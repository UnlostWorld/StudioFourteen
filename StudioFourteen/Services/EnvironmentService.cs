// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Services;

using PropertyChanged.SourceGenerator;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using StudioFourteen.Plugin;
using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using StudioFourteen.Utilities;
using StudioFourteen.Interop;

public partial class EnvironmentService
	: ServiceBase
{
	private long lastEorzeaTime;
	private long? nextEorzeaTime;

	[Notify] private bool isInTitleScreen;
	[Notify] private bool canChangeTerritory;
	[Notify] private TerritoryType? currentTerritory;
	[Notify] private Weather? currentWeather;
	[Notify] private bool freezeTime = false;
	[Notify] private string time = string.Empty;

	public long EorzeaTime
	{
		get
		{
			if (this.nextEorzeaTime != null)
				return (long)this.nextEorzeaTime;

			return this.lastEorzeaTime;
		}

		set
		{
			this.nextEorzeaTime = value;
			this.RaisePropertyChanged();
		}
	}

	public int MinuteOfDay
	{
		get
		{
			long currentTime = this.EorzeaTime;
			long timeVal = currentTime % 2764800;
			long secondInDay = timeVal % 86400;
			int minuteOfDay = (int)(secondInDay / 60f);
			return minuteOfDay;
		}

		set
		{
			this.EorzeaTime = (value * 60) + (86400 * ((byte)this.DayOfMonth - 1));
			this.RaisePropertyChanged();
		}
	}

	public int DayOfMonth
	{
		get
		{
			long currentTime = this.EorzeaTime;
			long timeVal = currentTime % 2764800;
			int dayOfMonth = (int)(Math.Floor(timeVal / 86400f) + 1);
			return dayOfMonth;
		}

		set
		{
			this.EorzeaTime = (this.MinuteOfDay * 60) + (86400 * ((byte)value - 1));
			this.RaisePropertyChanged();
		}
	}

	public override void Attach()
	{
		base.Attach();

		Hooks.CreateScene.Enable(this.HandleCreateScene);
		Hooks.UpdateEorzeaTime.Enable(this.UpdateEorzeaTime);
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		Hooks.CreateScene.Disable();
		Hooks.UpdateEorzeaTime.Disable();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public void ChangeTerritory(TerritoryType territory)
	{
		if (!this.CanChangeTerritory)
			return;

		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			string background = territory.Bg.ExtractText();
			if (string.IsNullOrEmpty(background))
				return;

			Hooks.CreateScene.Original(background, territory.RowId, 0, 0, 0, -1, 0);
			this.CurrentTerritory = territory;
		});
	}

	public unsafe void ChangeWeather(Weather weather)
	{
		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			EnvManager* environmentManager = EnvManager.Instance();
			if (environmentManager == null)
				return;

			environmentManager->ActiveWeather = (byte)weather.RowId;
			environmentManager->TransitionTime = 0;
		});
	}

	protected unsafe void OnGameTick()
	{
		Framework* pFramework = Framework.Instance();
		if (pFramework == null)
			return;

		EnvManager* environmentManager = EnvManager.Instance();
		if (environmentManager == null)
			return;

		// Time
		long newEorzeaTime = pFramework->ClientTime.IsEorzeaTimeOverridden ? pFramework->ClientTime.EorzeaTimeOverride : pFramework->ClientTime.EorzeaTime;
		bool hasTimeChanged = this.nextEorzeaTime != this.lastEorzeaTime;
		this.lastEorzeaTime = newEorzeaTime;

		if (this.nextEorzeaTime != null)
		{
			pFramework->ClientTime.EorzeaTime = (long)this.nextEorzeaTime;

			if (pFramework->ClientTime.IsEorzeaTimeOverridden)
				pFramework->ClientTime.EorzeaTimeOverride = (long)this.nextEorzeaTime;

			this.nextEorzeaTime = null;
		}

		if (hasTimeChanged)
		{
			this.RaisePropertyChanged(nameof(this.EorzeaTime));
			this.RaisePropertyChanged(nameof(this.MinuteOfDay));
			this.RaisePropertyChanged(nameof(this.DayOfMonth));

			TimeSpan displayTime = TimeSpan.FromMinutes(this.MinuteOfDay);
			this.Time = string.Format("{0:D2}:{1:D2}", displayTime.Hours, displayTime.Minutes);
		}

		// Territory Change
		this.IsInTitleScreen = this.GetIsInTitleScreen();
		this.CanChangeTerritory = this.GetCanChangeTerritory();

		// Weather
		byte weatherId = environmentManager->ActiveWeather;
		this.CurrentWeather = this.Services.GameData.GetRow<Weather>(weatherId);

		// Territory
		if (!this.IsInTitleScreen)
		{
			if (DalamudServices.ClientState == null)
				return;

			ushort territoryId = DalamudServices.ClientState.TerritoryType;
			this.CurrentTerritory = this.Services.GameData.GetRow<TerritoryType>(territoryId);
		}
	}

	private int HandleCreateScene(string backgroundPath, uint territoryId, IntPtr p3, uint layerFilterKey, IntPtr p5, int p6, uint contentFinderConditionId)
	{
		this.Log.Information($"Changed Scene: {backgroundPath}");

		this.CurrentTerritory = this.Services.GameData.GetRow<TerritoryType>(territoryId);

		return Hooks.CreateScene.Original(backgroundPath, territoryId, p3, layerFilterKey, p5, p6, contentFinderConditionId);
	}

	private void UpdateEorzeaTime(IntPtr a1, IntPtr a2)
	{
		if (this.FreezeTime)
			return;

		Hooks.UpdateEorzeaTime.Original(a1, a2);
	}

	private unsafe bool GetCanChangeTerritory()
	{
		TickService.VerifyGameTickThread();

		try
		{
			// Has the user already logged in? don't let them change zones, just for safeties sake.
			if (AgentLobby.Instance()->DataCenter != 0 || AgentLobby.Instance()->WorldId != 0)
				return false;

			// Is the user on the title screen?
			nint? charaSelect = DalamudServices.GameGui?.GetAddonByName("CharaSelect");
			nint? charaMake = DalamudServices.GameGui?.GetAddonByName("CharaMake");
			nint? titleDcWorldMap = DalamudServices.GameGui?.GetAddonByName("TitleDCWorldMap");
			if (charaMake != nint.Zero || charaSelect != nint.Zero || titleDcWorldMap != nint.Zero)
				return false;

			return !(DalamudServices.ClientState?.IsLoggedIn ?? false);
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error checking log in status");
			return false;
		}
	}

	private unsafe bool GetIsInTitleScreen()
	{
		TickService.VerifyGameTickThread();

		nint? titleMenu = DalamudServices.GameGui?.GetAddonByName("_TitleMenu");
		return titleMenu != null && titleMenu != nint.Zero;
	}
}

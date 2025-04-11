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
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using StudioFourteen.Plugin;
using System;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using Lumina.Excel.Sheets;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using StudioFourteen.Interop;
using System.Collections.Generic;

using Task = System.Threading.Tasks.Task;

public partial class EnvironmentService
	: ServiceBase
{
	private readonly Dictionary<int, string> dayNameLookup = new();
	private readonly Dictionary<int, string> monthNameLookup = new();

	[Notify] private long eorzeaTime;
	[Notify] private int dayOfMonth;
	[Notify] private int minuteOfDay;
	[Notify] private bool isInTitleScreen;
	[Notify] private bool canChangeTerritory;
	[Notify] private TerritoryType? currentTerritory;
	[Notify] private Weather? currentWeather;
	[Notify] private bool freezeTime = false;

	[Notify] private string displayTime = string.Empty;
	[Notify] private string displayMonth = string.Empty;

	private bool isUpdatingEorzeaTime = false;

	public override async Task Start()
	{
		for (int i = 0; i < 12; i++)
		{
			this.monthNameLookup[i] = Resources.Find($"LOC_Time_Month_{i}", i.ToString());
		}

		for (int i = 0; i < 32; i++)
		{
			this.dayNameLookup[i] = Resources.Find($"LOC_Time_Day_{i}", i.ToString());
		}

		await base.Start();
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
		if (this.freezeTime)
		{
			if (pFramework->ClientTime.IsEorzeaTimeOverridden)
			{
				pFramework->ClientTime.EorzeaTimeOverride = (long)this.eorzeaTime;
			}
			else
			{
				pFramework->ClientTime.EorzeaTime = (long)this.eorzeaTime;
			}

			newEorzeaTime = (long)this.eorzeaTime;
		}

		this.EorzeaTime = newEorzeaTime;

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

	protected void OnEorzeaTimeChanged()
	{
		this.isUpdatingEorzeaTime = true;

		long currentTime = this.EorzeaTime;
		long timeVal = currentTime % 2764800;
		long secondInDay = timeVal % 86400;
		this.MinuteOfDay = (int)(secondInDay / 60f);
		this.DayOfMonth = (int)Math.Floor(timeVal / 86400f);

		TimeSpan displayTime = TimeSpan.FromMinutes(this.MinuteOfDay);

		int hours = displayTime.Hours;
		bool isPm = hours > 12;
		if (isPm)
			hours -= 12;

		int month = DateTime.UtcNow.Month - 1;

		this.DisplayTime = $"{hours}:{displayTime.Minutes.ToString("D2")}{(isPm ? "pm" : "am")}";

		if (this.monthNameLookup.ContainsKey(month))
			this.displayMonth = $"{this.monthNameLookup[month]}, 1577";

		this.isUpdatingEorzeaTime = false;
	}

	protected void OnDayOfMonthChanged(int oldValue, int newValue)
	{
		if (this.isUpdatingEorzeaTime)
			return;

		this.EorzeaTime = (this.MinuteOfDay * 60) + (86400 * (byte)newValue);
	}

	protected void OnMinuteOfDayChanged(int oldValue, int newValue)
	{
		if (this.isUpdatingEorzeaTime)
			return;

		this.EorzeaTime = (newValue * 60) + (86400 * (byte)this.DayOfMonth);
	}

	private int HandleCreateScene(string backgroundPath, uint territoryId, IntPtr p3, uint layerFilterKey, IntPtr p5, int p6, uint contentFinderConditionId)
	{
		this.Log.Information($"Changed Scene: {backgroundPath}");

		this.CurrentTerritory = this.Services.GameData.GetRow<TerritoryType>(territoryId);

		return Hooks.CreateScene.Original(backgroundPath, territoryId, p3, layerFilterKey, p5, p6, contentFinderConditionId);
	}

	private void UpdateEorzeaTime(IntPtr a1, IntPtr a2)
	{
		if (this.freezeTime)
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

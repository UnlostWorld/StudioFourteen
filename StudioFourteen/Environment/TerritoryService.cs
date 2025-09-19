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

namespace StudioFourteen.Environment;

using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using StudioFourteen.Plugin;
using System;
using Lumina.Excel.Sheets;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using StudioFourteen.GameData.Library;

[Service]
public partial class TerritoryService
	: ServiceBase
{
	private bool isReadingTerritory = false;

	public delegate void TerritoryChangedDelegate(TerritoryTypeLibraryEntry? newTerritory);
	public event TerritoryChangedDelegate? TerritoryChanged;

	[Bind] public partial bool IsInTitleScreen { get; set; }
	[Bind] public partial bool CanChangeTerritory { get; set; }
	[Bind] public partial TerritoryTypeLibraryEntry? CurrentTerritory { get; set; }

	public override void Attach()
	{
		base.Attach();

		Hooks.CreateScene.Enable(this.HandleCreateScene);
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		Hooks.CreateScene.Disable();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public void ChangeTerritory(uint territoryId)
	{
		TerritoryTypeLibraryEntry? territory = this.Services.GameData.GetLibraryEntry<TerritoryTypeLibraryEntry>(territoryId);
		if (territory == null)
			return;

		this.ChangeTerritory(territory.Territory);
	}

	public void ChangeTerritory(string background)
	{
		Hooks.CreateScene.Original(background, 0, 0, 0, 0, -1, 0);
	}

	public void ChangeTerritory(TerritoryType territory)
	{
		TickService.VerifyGameTickThread();

		if (!this.CanChangeTerritory)
			return;

		string background = territory.Bg.ExtractText();
		if (string.IsNullOrEmpty(background))
			return;

		Hooks.CreateScene.Original(background, territory.RowId, 0, 0, 0, -1, 0);
	}

	public unsafe bool GetIsInTitleScreen()
	{
		TickService.VerifyGameTickThread();

		nint? titleMenu = DalamudServices.GameGui?.GetAddonByName("_TitleMenu");
		return titleMenu != null && titleMenu != nint.Zero;
	}

	protected unsafe void OnGameTick()
	{
		// Territory Change
		this.IsInTitleScreen = this.GetIsInTitleScreen();
		this.CanChangeTerritory = this.GetCanChangeTerritory();

		// Territory
		if (!this.IsInTitleScreen)
		{
			if (DalamudServices.ClientState == null)
				return;

			ushort territoryId = DalamudServices.ClientState.TerritoryType;

			if (this.CurrentTerritory?.RowId != territoryId)
			{
				this.isReadingTerritory = true;
				this.CurrentTerritory = this.Services.GameData.GetLibraryEntry<TerritoryTypeLibraryEntry>(territoryId);
				this.isReadingTerritory = false;
			}
		}
	}

	private int HandleCreateScene(string backgroundPath, uint territoryId, IntPtr p3, uint layerFilterKey, IntPtr p5, int p6, uint contentFinderConditionId)
	{
		this.Log.Information($">> {backgroundPath} - {territoryId} - {layerFilterKey}");

		this.isReadingTerritory = true;
		this.CurrentTerritory = this.Services.GameData.GetLibraryEntry<TerritoryTypeLibraryEntry>(territoryId);
		this.isReadingTerritory = false;

		return Hooks.CreateScene.Original(backgroundPath, territoryId, p3, layerFilterKey, p5, p6, contentFinderConditionId);
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

	private void OnCurrentTerritoryChanged(TerritoryTypeLibraryEntry? oldValue, TerritoryTypeLibraryEntry? newValue)
	{
		this.TerritoryChanged?.Invoke(newValue);

		if (this.isReadingTerritory || newValue == null)
			return;

		this.Services.Tick.Dispatch(TickService.Channels.GameTick, () => this.ChangeTerritory(newValue.Territory));
	}
}

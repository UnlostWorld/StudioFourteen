// Title Edit
// https://github.com/RokasKil/TitleEdit/blob/d9e83314ee4be29e3d35c3732e48ccdb0983d96d/TitleEdit/PluginServices/Lobby/LobbyService.cs#L169

namespace StudioFourteen.Services;

using PropertyChanged.SourceGenerator;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using StudioFourteen.Plugin;
using System;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using StudioFourteen.Mvm;
using Lumina.Excel.Sheets;

public partial class EnvironmentService
	: ServiceBase
{
	private Hook<OnCreateScene>? createSceneHook;
	[Notify] private TerritoryType? currentTerritory;
	[Notify] private Weather? currentWeather;

	private delegate int OnCreateScene(string p1, uint p2, IntPtr p3, uint p4, IntPtr p5, int p6, uint p7);

	public unsafe bool IsInTitleScreen
	{
		get
		{
			nint? titleMenu = DalamudServices.GameGui?.GetAddonByName("_TitleMenu");
			return titleMenu != null && titleMenu != nint.Zero;
		}
	}

	[AutoNotify]
	public unsafe bool CanChangeTerritory
	{
		get
		{
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
	}

	public override void Attach()
	{
		base.Attach();

		this.createSceneHook = InteropService.HookFromSignature<OnCreateScene>("E8 ?? ?? ?? ?? 66 89 1D ?? ?? ?? ?? E9 ?? ?? ?? ??", this.HandleCreateScene);
		this.createSceneHook?.Enable();
	}

	public override void Detach()
	{
		this.createSceneHook?.Dispose();
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

			this.createSceneHook?.Original(background, territory.RowId, 0, 0, 0, -1, 0);
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
		});
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		EnvManager* environmentManager = EnvManager.Instance();
		if (environmentManager == null)
			return;

		byte weatherId = environmentManager->ActiveWeather;
		this.CurrentWeather = this.Services.GameData.GetRow<Weather>(weatherId);

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

		if (this.createSceneHook == null)
			return 0;

		this.CurrentTerritory = this.Services.GameData.GetRow<TerritoryType>(territoryId);

		return this.createSceneHook.Original(backgroundPath, territoryId, p3, layerFilterKey, p5, p6, contentFinderConditionId);
	}
}

//// TitleEdit
//// https://github.com/lmcintyre/TitleEditPlugin/tree/main/TitleEdit
//// https://github.com/lmcintyre/TitleEditPlugin/blob/main/TitleEdit/TitleEditAddressResolver.cs

namespace ScreenshotStudio.Studio;

using Dalamud.Game.ClientState;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using ScreenshotStudio.GameData.Excel;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using ScreenshotStudio.Windows;
using System;
using System.Windows;

public partial class ZoneWindow : PanelWindow
{
	private Hook<OnCreateScene>? createSceneHook;

	private delegate int OnCreateScene(string p1, uint p2, IntPtr p3, uint p4, IntPtr p5, int p6, uint p7);

	[AutoNotify]
	public unsafe bool CanChangeZone
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
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error checking log in status");
				return false;
			}
		}
	}

	protected override void OnOpened()
	{
		this.createSceneHook = InteropService.HookFromSignature<OnCreateScene>("E8 ?? ?? ?? ?? 66 89 1D ?? ?? ?? ?? E9 ?? ?? ?? ??", this.HandleCreateScene);
		this.createSceneHook?.Enable();

		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.createSceneHook?.Dispose();
		base.OnClosed();
	}

	private void LoadScene(string path)
	{
		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			this.Log.Information($"Changing Scene: {path}");
			this.createSceneHook?.Original(path, 0, 0, 0, 0, -1, 0);
		});
	}

	private int HandleCreateScene(string backgroundPath, uint p2, IntPtr p3, uint p4, IntPtr p5, int p6, uint p7)
	{
		this.Log.Information($"Changed Scene: {backgroundPath}");

		if (this.createSceneHook == null)
			return 0;

		return this.createSceneHook.Original(backgroundPath, p2, p3, p4, p5, p6, p7);
	}

	private void OnChangeZoneClicked(object sender, RoutedEventArgs e)
	{
		if (DalamudServices.ClientState == null || DalamudServices.ClientState.IsLoggedIn)
			return;

		TagCollection defaultTags = new();
		LibraryModal.Show<Territory>(
			this,
			"Change Zone",
			defaultTags,
			null,
			(territory, isFinal) =>
			{
				if (!isFinal || territory == null || territory.Background == null)
					return;

				this.LoadScene(territory.Background);
			});
	}
}

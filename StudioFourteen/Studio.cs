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

namespace StudioFourteen;

using System;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Logging;
using StudioFourteen.Services.Platform;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Tick;
using StudioFourteen.Services.Avalonia;
using StudioFourteen.Services.Input;
using StudioFourteen.Services.Serialization;
using StudioFourteen.Services;
using StudioFourteen.Services.Library;
using StudioFourteen.Services.Scene;
using StudioFourteen.Services.Portraits;
using StudioFourteen.Services.Interop;
using StudioFourteen.Interface;

public sealed class Studio : IDalamudPlugin
{
	public Studio(IDalamudPluginInterface pluginInterface)
	{
		Instance = this;
		IsDisposed = false;
		IsInitialized = false;

		Json = new();
		Log = new();
		Platform = new();
		Tick = new();
		Content = new();
		Input = new();
		Camera = new();
		Rendering = new();
		Avalonia = new();
		Window = new();
		Library = new();
		Scene = new();
		Portraits = new();
		Redraw = new();
		Interface = new();
		Commands = new();

		IsInitialized = true;

		Hooks.EnforceKind.Enable(this.EnforceKindRestrictionsDetour);

		Commands.AddCommand("s14", "Open Studio Fourteen", Open);
		PluginInterface.UiBuilder.OpenMainUi += Open;
		PluginInterface.UiBuilder.OpenConfigUi += Open;

		// Open the UI if this is a debug build.
#if DEBUG
		Open();
#endif
	}

	public static bool IsDisposed { get; private set; } = false;
	public static bool IsInitialized { get; private set; } = false;

	public static Studio Instance { get; private set; } = null!;

	public static LoggingService Log { get; private set; } = null!;
	public static JsonSerializer Json { get; private set; } = null!;
	public static RenderingService Rendering { get; private set; } = null!;
	public static ContentService Content { get; private set; } = null!;
	public static TickService Tick { get; private set; } = null!;
	public static CameraService Camera { get; private set; } = null!;
	public static AvaloniaService Avalonia { get; private set; } = null!;
	public static PlatformService Platform { get; private set; } = null!;
	public static InputService Input { get; private set; } = null!;
	public static WindowService Window { get; private set; } = null!;
	public static LibraryService Library { get; private set; } = null!;
	public static SceneService Scene { get; private set; } = null!;
	public static PortraitService Portraits { get; private set; } = null!;
	public static RedrawService Redraw { get; private set; } = null!;
	public static InterfaceService Interface { get; private set; } = null!;
	public static CommandService Commands { get; private set; } = null!;

	[PluginService] public static IPluginLog DalamudLog { get; private set; } = null!;
	[PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] public static ICommandManager DalamudCommandManager { get; private set; } = null!;
	[PluginService] public static IDataManager DataManager { get; private set; } = null!;
	[PluginService] public static IClientState ClientState { get; private set; } = null!;
	[PluginService] public static ISigScanner SigScanner { get; private set; } = null!;
	[PluginService] public static IFramework Framework { get; private set; } = null!;
	[PluginService] public static IKeyState KeyState { get; private set; } = null!;
	[PluginService] public static IGameGui GameGui { get; private set; } = null!;
	[PluginService] public static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;
	[PluginService] public static IGameInteropProvider InteropProvider { get; private set; } = null!;
	[PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
	[PluginService] public static IObjectTable ObjectTable { get; private set; } = null!;
	[PluginService] public static IAddonEventManager AddonEventManager { get; private set; } = null!;

	public static void Open()
	{
		Interface.Open();
	}

	public static void Close()
	{
		Interface.Close();
		Scene.ClearSelection();
	}

	public void Dispose()
	{
		Instance = null!;
		IsDisposed = true;

		try
		{
			Interface.Dispose();
			Log.Dispose();
			Platform.Dispose();
			Rendering.Dispose();
			Content.Dispose();
			Tick.Dispose();
			Camera.Dispose();
			Avalonia.Dispose();
			Input.Dispose();
			Window.Dispose();
			Library.Dispose();
			Scene.Dispose();
			Portraits.Dispose();
			Redraw.Dispose();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error disposing services");
		}

		Hooks.EnforceKind.Disable();
	}

	private byte EnforceKindRestrictionsDetour(nint a1, nint a2)
	{
		// always allow npc values.
		////return this.enforceKindRestrictionsHook.Original(a1, a2);
		return 0;
	}
}
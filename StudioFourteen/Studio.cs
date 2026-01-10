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
using Dalamud.Utility;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Rendering;
using StudioFourteen.Services.Tick;

public sealed class Studio : IDalamudPlugin
{
	public Studio(IDalamudPluginInterface pluginInterface)
	{
		Tick = new();
		Content = new();
		Camera = new();
		Rendering = new();
	}

	public static bool IsDisposed { get; private set; } = false;

	public static RenderingService Rendering { get; private set; } = null!;
	public static ContentService Content { get; private set; } = null!;
	public static TickService Tick { get; private set; } = null!;
	public static CameraService Camera { get; private set; } = null!;

	[PluginService] public static IPluginLog Log { get; private set; } = null!;
	[PluginService] public static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
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

	public void Dispose()
	{
		IsDisposed = true;

		try
		{
			Rendering.Dispose();
			Content.Dispose();
			Tick.Dispose();
			Camera.Dispose();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error disposing services");
		}
	}
}
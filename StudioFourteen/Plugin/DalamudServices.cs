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

namespace StudioFourteen.Plugin;

using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

public class DalamudServices
{
	public static bool IsAlive => Log != null;

	[PluginService] public static IPluginLog? Log { get; private set; }
	[PluginService] public static IDalamudPluginInterface? PluginInterface { get; private set; }
	[PluginService] public static ICommandManager? CommandManager { get; private set; }
	[PluginService] public static IDataManager? DataManager { get; private set; }
	[PluginService] public static IClientState? ClientState { get; private set; }
	[PluginService] public static ISigScanner? SigScanner { get; private set; }
	[PluginService] public static IFramework? Framework { get; private set; }
	[PluginService] public static IKeyState? KeyState { get; private set; }
	[PluginService] public static IGamepadState? GamepadState { get; private set; }
	[PluginService] public static IGameGui? GameGui { get; private set; }
	[PluginService] public static ITextureSubstitutionProvider? TextureSubstitutionProvider { get; private set; }
	[PluginService] public static IGameInteropProvider? InteropProvider { get; private set; }
	[PluginService] public static ITextureProvider? TextureProvider { get; private set; }
	[PluginService] public static IGameConfig? GameConfig { get; private set; }
	[PluginService] public static IContextMenu? ContextMenu { get; private set; }
}

//// Special thanks to Ktisis, @chirpxiv
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Services.cs

namespace ScreenshotStudio.Plugin;

using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Runtime.InteropServices;

public class DalamudServices
{
	[PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static IDataManager DataManager { get; private set; } = null!;
	[PluginService] internal static IClientState ClientState { get; private set; } = null!;
	[PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
	[PluginService] internal static SigScanner SigScanner { get; private set; } = null!;
	[PluginService] internal static IFramework Framework { get; private set; } = null!;
	[PluginService] internal static IKeyState KeyState { get; private set; } = null!;
	[PluginService] internal static IGameGui GameGui { get; private set; } = null!;
	[PluginService] internal static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;
	[PluginService] internal static IGameInteropProvider InteropProvider { get; private set; } = null!;
	[PluginService] internal static IPluginLog Log { get; private set; } = null!;

	internal static unsafe CameraManager* Camera { get; private set; } = CameraManager.Instance();

	public static TDelegate DelegateFromSignature<TDelegate>(string sig) => Marshal.GetDelegateForFunctionPointer<TDelegate>(SigScanner.ScanText(sig));
}

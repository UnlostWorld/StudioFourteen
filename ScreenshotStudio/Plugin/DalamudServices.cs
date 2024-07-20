namespace ScreenshotStudio.Plugin;

using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System;
using System.Runtime.InteropServices;

public class DalamudServices
{
	[PluginService] public static IPluginLog? Log { get; private set; }
	[PluginService] public static IDalamudPluginInterface? PluginInterface { get; private set; }
	[PluginService] public static ICommandManager? CommandManager { get; private set; }
	[PluginService] public static IDataManager? DataManager { get; private set; }
	[PluginService] public static IClientState? ClientState { get; private set; }
	[PluginService] public static IObjectTable? ObjectTable { get; private set; }
	[PluginService] public static ISigScanner? SigScanner { get; private set; }
	[PluginService] public static IFramework? Framework { get; private set; }
	[PluginService] public static IKeyState? KeyState { get; private set; }
	[PluginService] public static IGameGui? GameGui { get; private set; }
	[PluginService] public static ITextureSubstitutionProvider? TextureSubstitutionProvider { get; private set; }
	[PluginService] public static IGameInteropProvider? InteropProvider { get; private set; }
	[PluginService] public static ITextureProvider? TextureProvider { get; private set; }

	internal static unsafe CameraManager* Camera { get; private set; } = CameraManager.Instance();

	public static TDelegate? DelegateFromSignature<TDelegate>(string sig)
		where TDelegate : System.Delegate
	{
		if (SigScanner == null)
			return null;

		try
		{
			nint address = SigScanner.ScanText(sig);
			return Marshal.GetDelegateForFunctionPointer<TDelegate>(address);
		}
		catch (Exception ex)
		{
			Logging.ForContext<DalamudServices>().Error(ex, "Error creating delegate from signature");
			return null;
		}
	}
}

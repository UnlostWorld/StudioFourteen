// © XivTools.
// Licensed under the MIT license.

//// Special thanks to Ktisis, @chirpxiv
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Services.cs

namespace ScreenshotStudio.Plugin;

using Dalamud.Data;
using Dalamud.Game;
using Dalamud.Game.ClientState;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

public class DalamudServices
{
	[PluginService] internal static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService] internal static CommandManager CommandManager { get; private set; } = null!;
	[PluginService] internal static DataManager DataManager { get; private set; } = null!;
	[PluginService] internal static ClientState ClientState { get; private set; } = null!;
	[PluginService] internal static ObjectTable ObjectTable { get; private set; } = null!;
	[PluginService] internal static SigScanner SigScanner { get; private set; } = null!;
	[PluginService] internal static Framework Framework { get; private set; } = null!;
	[PluginService] internal static KeyState KeyState { get; private set; } = null!;
	[PluginService] internal static GameGui GameGui { get; private set; } = null!;
	[PluginService] internal static ITextureSubstitutionProvider TextureSubstitutionProvider { get; private set; } = null!;

	internal static unsafe CameraManager* Camera { get; private set; } = CameraManager.Instance;
}

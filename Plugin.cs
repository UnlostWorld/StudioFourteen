namespace ScreenshotStudio;

using Dalamud.Game.Gui;
using Dalamud.Game;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using Dalamud.Game.Command;

public sealed class Plugin : IDalamudPlugin
{
	public string Name => "Screenshot Studio";

	[PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;
	[PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

	private MainWindow mw;

	public Plugin()
	{
		PluginLog.Information($"Screenshot Studio started");

		this.mw = new();
		this.mw.Show();
	}

	public void Dispose()
	{
		PluginLog.Information($"Screenshot Studio stopped");
		this.mw.Close();
	}
}

namespace ScreenshotStudio.Plugin;

using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Logging;
using Dalamud.Plugin;
using ScreenshotStudio.Studio;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using Serilog;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;

public sealed class DalamudPlugin : IDalamudPlugin
{
    public string Name => "Screenshot Studio";

    [PluginService][RequiredVersion("1.0")] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static CommandManager CommandManager { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static ChatGui ChatGui { get; private set; } = null!;
    [PluginService][RequiredVersion("1.0")] public static SigScanner SigScanner { get; private set; } = null!;

    HelloWorldWindow? wnd;

	public DalamudPlugin() => Task.Run(Start);
    public void Dispose() => Task.Run(Stop);
    
    private async Task Start()
    {
		// Hard reference our required sattelite assemblies to make sure dalamuds plugin loader picks them up.
		Log.Information($"Ensure assembly XivToolWpf {typeof(XivToolsWpf.Dispatch).Assembly}");
		Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		Log.Information($"Ensure assembly FontAwesome Pro {typeof(FontAwesome.Sharp.Pro.Icon).Assembly}");

        // Get the Xiv process for window manipulation.
        // NOTE: if we _dont_ log out the value here, then things break. I don't know why.
		XivWindow.Process = Process.GetCurrentProcess();
        Log.Information($"Ensure XivProcess {XivWindow.Process} - {XivWindow.Process.MainWindowHandle} - {XivWindow.Process.MainWindowTitle}");

		wnd = await Panel.ShowAsync<HelloWorldWindow>();
    }

	private async Task Stop()
	{
        if (wnd != null)
        {
            await wnd.CloseAsync();
        }
	}
}

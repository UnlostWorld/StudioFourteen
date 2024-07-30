namespace ScreenshotStudio.Plugin;

using Dalamud.Plugin;
using ScreenshotStudio.Utilities;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class DalamudPlugin : IDalamudPlugin
{
	private readonly ServiceManager services = new();

	public DalamudPlugin(IDalamudPluginInterface pluginInterface)
	{
		// Hard reference our required satellite assemblies to make sure dalamuds plugin loader picks them up.
		this.Log.Information($"Ensure assembly XivToolWpf {typeof(WpfUtils.Dispatch).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		this.Log.Information($"Ensure assembly VirtualizingWrapPanel Pro {typeof(WpfToolkit.Controls.VirtualizingWrapPanel).Assembly}");

		// Get the Xiv process for window manipulation.
		// NOTE: if we _don't_ log out the value here, then things break. I don't know why.
		XivWindow.Process = Process.GetCurrentProcess();
		this.Log.Information($"Ensure XivProcess {XivWindow.Process} - {XivWindow.Process.MainWindowHandle} - {XivWindow.Process.MainWindowTitle}");

		pluginInterface.Create<DalamudServices>();
		Task.Run(this.services.Start);
	}

	public string Name => "Screenshot Studio";
	public ILogger Log => Logging.ForContext<DalamudPlugin>();

	public void Dispose()
	{
		this.services.Stop().Wait();
	}
}
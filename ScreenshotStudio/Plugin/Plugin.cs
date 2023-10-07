namespace ScreenshotStudio.Plugin;

using Dalamud.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;

public sealed class DalamudPlugin : IDalamudPlugin
{
	public DalamudPlugin(DalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<DalamudServices>();
		Task.Run(this.Start);
	}

	public string Name => "Screenshot Studio";
	public ILogger Log => Logging.ForContext<DalamudPlugin>();

	public void Dispose() => Task.Run(this.Stop);

	private async Task Start()
	{
		Logging.Init();

		// Hard reference our required sattelite assemblies to make sure dalamuds plugin loader picks them up.
		this.Log.Information($"Ensure assembly XivToolWpf {typeof(WpfUtils.Dispatch).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome Pro {typeof(FontAwesome.Sharp.Pro.Icon).Assembly}");

		// Get the Xiv process for window manipulation.
		// NOTE: if we _dont_ log out the value here, then things break. I don't know why.
		XivWindow.Process = Process.GetCurrentProcess();
		this.Log.Information($"Ensure XivProcess {XivWindow.Process} - {XivWindow.Process.MainWindowHandle} - {XivWindow.Process.MainWindowTitle}");

		Alloc.Init();

		await ServiceManager.Instance.Start();
	}

	private async Task Stop()
	{
		await ServiceManager.Instance.Stop();
		Alloc.Dispose();
	}
}
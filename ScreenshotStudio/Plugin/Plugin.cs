// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Plugin;

using Dalamud.Game;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using Serilog;
using System.Collections.Generic;
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

	public void Dispose() => Task.Run(this.Stop);

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

		await ServiceManager.Instance.Start();
	}

	private async Task Stop()
	{
		await ServiceManager.Instance.Stop();
	}
}

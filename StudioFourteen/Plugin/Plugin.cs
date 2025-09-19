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

using Dalamud.Game.Command;
using Dalamud.Plugin;
using Dalamud.Utility;
using Serilog;
using StudioFourteen.Settings;
using System;
using System.Threading.Tasks;
using StudioFourteen.Extensions;

#if DALAMUD

public sealed class DalamudPlugin : IDalamudPlugin
{
	public readonly ServiceManager Services = new();

	public DalamudPlugin(IDalamudPluginInterface pluginInterface)
	{
		Instance = this;

		// Hard reference our required satellite assemblies to make sure dalamud's plugin loader picks them up.
		this.Log.Information($"Ensure assembly  {typeof(StudioFourteen.Dispatch).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		this.Log.Information($"Ensure assembly VirtualizingWrapPanel Pro {typeof(WpfToolkit.Controls.VirtualizingWrapPanel).Assembly}");
		this.Log.Information($"Ensure assembly SVGImage {typeof(SVGImage.SVG.SVGImage).Assembly}");
		this.Log.Information($"Ensure assembly WebView2 {typeof(Microsoft.Web.WebView2.Wpf.WebView2).Assembly}");

		// Ensure the pack URI scheme got registered
		this.Log.Information($"Ensure Pack URI {System.IO.Packaging.PackUriHelper.UriSchemePack}");

		pluginInterface.Create<DalamudServices>();

		this.Log.Information("$IsWine: {DalamudServices.IsWine}");

		if (DalamudServices.CommandManager != null)
		{
			CommandInfo command = new(this.OnS14Command);
			command.HelpMessage = "Toggle Studio Fourteen";
			command.ShowInHelp = true;

			DalamudServices.CommandManager.AddHandler("/s14", command);
		}

		if (DalamudServices.PluginInterface != null)
		{
			DalamudServices.PluginInterface.UiBuilder.OpenMainUi += this.OnDalamudOpenMainUi;
			DalamudServices.PluginInterface.UiBuilder.OpenConfigUi += this.OnDalamudOpenConfigUi;
		}

		Task.Run(this.Services.Start);
	}

	public static IDalamudPlugin? Instance { get; private set; }
	public string Name => "Studio Fourteen";
	public ILogger Log => Logging.ForContext<DalamudPlugin>();

	public void Dispose()
	{
		// Dispose services before stopping them.
		// We do this because dispose is synchronous, while
		// stop is async, and we want to get any lingering hooks
		// out before dalamud moves on from this method.
		this.Services.Dispose();
		Logging.Dispose();

		this.Services.Stop().RunAsynchronously();
	}

	private void OnDalamudOpenMainUi()
	{
		this.Services.Studio.OpenStudio();
	}

	private void OnDalamudOpenConfigUi()
	{
		this.Services.Panels.GamePanels.SetIsOpen<SettingsPanel>(true, true);
	}

	private void OnS14Command(string command, string arguments)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.Services.Studio.CloseStudio();
		}
		else
		{
			this.Services.Studio.OpenStudio();
		}
	}
}

#endif
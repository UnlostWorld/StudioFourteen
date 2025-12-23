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
using Serilog;
using StudioFourteen.Settings;
using System.Threading.Tasks;

#if DALAMUD

public sealed class DalamudPlugin : IDalamudPlugin
{
	public readonly ServiceManager Services = new();

	public DalamudPlugin(IDalamudPluginInterface pluginInterface)
	{
		Instance = this;

		pluginInterface.Create<DalamudServices>();

		this.Log.Information($"IsWine: {DalamudServices.IsWine}");

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
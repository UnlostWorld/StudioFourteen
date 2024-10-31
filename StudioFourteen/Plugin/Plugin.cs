namespace StudioFourteen.Plugin;

using Dalamud.Plugin;
using StudioFourteen.Utilities;
using Serilog;
using System.Diagnostics;
using System.Threading.Tasks;
using StudioFourteen.Settings;
using Dalamud.Game.Command;

public sealed class DalamudPlugin : IDalamudPlugin
{
	public readonly ServiceManager Services = new();

	public DalamudPlugin(IDalamudPluginInterface pluginInterface)
	{
		// Hard reference our required satellite assemblies to make sure dalamuds plugin loader picks them up.
		this.Log.Information($"Ensure assembly WpfUtils {typeof(WpfUtils.Dispatch).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		this.Log.Information($"Ensure assembly VirtualizingWrapPanel Pro {typeof(WpfToolkit.Controls.VirtualizingWrapPanel).Assembly}");

		// Get the Xiv process for window manipulation.
		// NOTE: if we _don't_ log out the value here, then things break. I don't know why.
		XivWindow.Process = Process.GetCurrentProcess();
		this.Log.Information($"Ensure XivProcess {XivWindow.Process} - {XivWindow.Process.MainWindowHandle} - {XivWindow.Process.MainWindowTitle}");

		pluginInterface.Create<DalamudServices>();

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

	public string Name => "Studio Fourteen";
	public ILogger Log => Logging.ForContext<DalamudPlugin>();

	public void Dispose()
	{
		this.Services.Stop().Wait();
	}

	private void OnDalamudOpenMainUi()
	{
		this.Services.Studio.OpenStudio();
	}

	private void OnDalamudOpenConfigUi()
	{
		this.Services.Panels.SetIsOpen<SettingsPanel>(true);
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
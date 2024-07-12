namespace ScreenshotStudio.Plugin;

using Dalamud.Plugin;
using Serilog;
using System.Threading.Tasks;

public sealed class DalamudPlugin : IDalamudPlugin
{
	private readonly ServiceManager services = new();

	public DalamudPlugin(IDalamudPluginInterface pluginInterface)
	{
		pluginInterface.Create<DalamudServices>();
		Task.Run(this.services.Start);
	}

	public string Name => "Screenshot Studio";
	public ILogger Log => Logging.ForContext<DalamudPlugin>();

	public void Dispose()
	{
		Task.Run(this.services.Stop);
	}
}
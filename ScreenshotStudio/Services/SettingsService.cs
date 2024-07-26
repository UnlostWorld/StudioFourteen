namespace ScreenshotStudio.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Configuration;
using ScreenshotStudio.Plugin;

public class SettingsService : ServiceBase
{
	public Configuration Current { get; private set; } = new Configuration();

	public override Task Initialize()
	{
		Configuration? current = DalamudServices.PluginInterface?.GetPluginConfig() as Configuration;
		if (current != null)
		{
			this.Current = current;
		}

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Save();
		return base.Shutdown();
	}

	public void Save()
	{
		DalamudServices.PluginInterface?.SavePluginConfig(this.Current);
	}

	public class Configuration : IPluginConfiguration
	{
		public int Version { get; set; } = 0;
		public List<string> OpenPanels { get; set; } = new();
		public bool IsOpen { get; set; } = false;
		public Dictionary<string, string> PanelPersistence { get; set; } = new();
	}
}
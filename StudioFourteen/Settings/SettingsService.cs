namespace StudioFourteen.Settings;

using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using StudioFourteen.Input;
using StudioFourteen.Plugin;
using StudioFourteen.Save;
using StudioFourteen.Serialization;
using StudioFourteen.Services;

public class SettingsService : ServiceBase
{
	public Configuration Current { get; private set; } = new Configuration();

	public override Task Initialize()
	{
		Configuration? current = null;

		if (DalamudServices.PluginInterface != null)
		{
			current = DalamudServices.PluginInterface?.GetPluginConfig() as Configuration;
		}
		else
		{
			if (File.Exists("config.json"))
			{
				current = Serializer.Deserialize<Configuration>(File.ReadAllText("config.json"));
			}
		}

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
		if (DalamudServices.PluginInterface != null)
		{
			DalamudServices.PluginInterface?.SavePluginConfig(this.Current);
		}
		else
		{
			string json = Serializer.Serialize(this.Current);
			File.WriteAllText("config.json", json);
		}
	}

	public class Configuration : IPluginConfiguration
	{
		public int Version { get; set; } = 0;
		public List<string> OpenPanels { get; set; } = new();
		public Dictionary<string, string> PanelPersistence { get; set; } = new();

		// Files
		public string? LastSaveDirectory { get; set; }
		public SaveService.SaveConfiguration SaveConfig { get; set; } = new();

		public string? DefaultAuthor { get; set; }
		public string? DefaultVersion { get; set; } = "1.0";

		// Input
		public bool EnableKeyBinds { get; set; } = true;
		public Dictionary<KeyBindEvents, KeyBind> CustomKeyBinds { get; set; } = new();

		// SPA
		public bool IsSpa { get; set; } = false;

		// Library
		public HashSet<string> Favorites { get; set; } = new();
	}
}
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
		public bool IsOpen { get; set; } = false;
		public Dictionary<string, string> PanelPersistence { get; set; } = new();

		// Files
		public string? LastSaveDirectory { get; set; }
		public SaveService.SaveConfiguration SaveConfig { get; set; } = new();

		public string? DefaultAuthor { get; set; }
		public string? DefaultVersion { get; set; } = "1.0";

		// Input
		public bool EnableKeyBinds { get; set; } = true;
		public Dictionary<KeyBindEvents, KeyBind> Keys { get; set; } = new()
		{
			{ KeyBindEvents.InvokeQuickSearch, new(VirtualKey.Q, ModifierKeys.Shift) },
			{ KeyBindEvents.Save, new(VirtualKey.S, ModifierKeys.Control) },
			{ KeyBindEvents.SaveAs, new(VirtualKey.S, ModifierKeys.Control | ModifierKeys.Shift) },

			// Free Camera
			{ KeyBindEvents.FreeCamera_MoveForwards, new(VirtualKey.W) },
			{ KeyBindEvents.FreeCamera_MoveBack, new(VirtualKey.S) },
			{ KeyBindEvents.FreeCamera_MoveLeft, new(VirtualKey.A) },
			{ KeyBindEvents.FreeCamera_MoveRight, new(VirtualKey.D) },
			{ KeyBindEvents.FreeCamera_MoveUp, new(VirtualKey.R) },
			{ KeyBindEvents.FreeCamera_MoveDown, new(VirtualKey.F) },
			{ KeyBindEvents.FreeCamera_YawLeft, new(VirtualKey.A, ModifierKeys.Shift) },
			{ KeyBindEvents.FreeCamera_YawRight, new(VirtualKey.D, ModifierKeys.Shift) },
			{ KeyBindEvents.FreeCamera_PitchUp, new(VirtualKey.W, ModifierKeys.Shift) },
			{ KeyBindEvents.FreeCamera_PitchDown, new(VirtualKey.S, ModifierKeys.Shift) },
			{ KeyBindEvents.FreeCamera_RollLeft, new(VirtualKey.Q) },
			{ KeyBindEvents.FreeCamera_RollRight, new(VirtualKey.E) },
		};

		// SPA
		public bool IsSpa { get; set; } = false;
	}
}
namespace ScreenshotStudio.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using ScreenshotStudio.Input;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Save;

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

		// Files
		public string? LastSaveDirectory { get; set; }
		public SaveService.SaveConfiguration SaveConfig { get; set; } = new();

		public string? DefaultAuthor { get; set; }
		public string? DefaultVersion { get; set; } = "1.0";

		// Input
		public bool EnableKeyBinds { get; set; } = true;
		public Dictionary<KeyBindEvents, KeyBind> KeyBinds { get; set; } = new()
		{
			{ KeyBindEvents.InvokeQuickSearch, new(VirtualKey.Q, false, false, true) },
			{ KeyBindEvents.Save, new(VirtualKey.S, true, false, false) },
			{ KeyBindEvents.SaveAs, new(VirtualKey.S, true, false, true) },
		};
	}
}
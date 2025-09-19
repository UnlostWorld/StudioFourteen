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

namespace StudioFourteen.Settings;

using StudioFourteen.Plugin;
using StudioFourteen.Serialization;
using StudioFourteen.Services;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

[Service]
public partial class SettingsService : ServiceBase
{
	private readonly FuncQueue saveQueue;
	private Configuration current = new Configuration();

	public SettingsService()
	{
		this.saveQueue = new(this.SaveImmediate, 500);
	}

	public delegate void SettingChangedDelegate(string settingName, object? newValue);

	public event SettingChangedDelegate? SettingChanged;

	public enum InterfacePresets
	{
		// Use the widget, show the target bar.
		FloatingInspector,

		// Use the inspector window, hide the target bar.
		SingleInspectorWindow,

		// Use the independent inspectors, show the target bar.
		DedicatedInspectorWindows,

		// Disable all windows, use the AIO window.
		AllInOne,
	}

	public Configuration Current
	{
		get => this.current;

		private set
		{
			if (this.current != null)
				this.current.PropertyChanged -= this.OnCurrentConfigPropertyChanged;

			this.current = value;

			if (this.current != null)
			{
				this.current.PropertyChanged += this.OnCurrentConfigPropertyChanged;
			}
		}
	}

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
			current.Validate();
			this.Current = current;
		}

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.SaveImmediate();
		return base.Shutdown();
	}

	public void Save()
	{
		this.saveQueue.Invoke();
	}

	public void SaveImmediate()
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

	public void SetPreset(InterfacePresets preset)
	{
		switch (preset)
		{
			case InterfacePresets.FloatingInspector:
			{
				this.current.WidgetMode = Configuration.WidgetModes.Inspector;
				this.current.EnableInspector = false;
				this.current.EnableDedicatedInspectors = false;
				this.current.AllInOne = Configuration.AioModes.Disabled;
				this.current.EnableTargetBar = true;
				break;
			}

			case InterfacePresets.SingleInspectorWindow:
			{
				this.current.WidgetMode = Configuration.WidgetModes.Disabled;
				this.current.EnableInspector = true;
				this.current.EnableDedicatedInspectors = false;
				this.current.AllInOne = Configuration.AioModes.Disabled;
				this.current.EnableTargetBar = false;
				break;
			}

			case InterfacePresets.DedicatedInspectorWindows:
			{
				this.current.WidgetMode = Configuration.WidgetModes.Disabled;
				this.current.EnableInspector = false;
				this.current.EnableDedicatedInspectors = true;
				this.current.AllInOne = Configuration.AioModes.Disabled;
				this.current.EnableTargetBar = true;
				break;
			}

			case InterfacePresets.AllInOne:
			{
				this.current.WidgetMode = Configuration.WidgetModes.Disabled;
				this.current.EnableInspector = false;
				this.current.EnableDedicatedInspectors = false;
				this.current.AllInOne = Configuration.AioModes.Always;
				this.current.EnableTargetBar = false;
				break;
			}
		}
	}

	private void OnCurrentConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == null)
			return;

		object? value = typeof(Configuration).GetProperty(e.PropertyName)?.GetValue(this.Current);
		this.SettingChanged?.Invoke(e.PropertyName, value);
		this.Save();
	}
}
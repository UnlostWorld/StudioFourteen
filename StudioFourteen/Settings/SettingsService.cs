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

using Dalamud.Configuration;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Library;
using StudioFourteen.Photos;
using StudioFourteen.Plugin;
using StudioFourteen.Save;
using StudioFourteen.Serialization;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using WpfUtils.Utils;

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

	private void OnCurrentConfigPropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (e.PropertyName == null)
			return;

		object? value = typeof(Configuration).GetProperty(e.PropertyName)?.GetValue(this.Current);
		this.SettingChanged?.Invoke(e.PropertyName, value);
		this.Save();
	}

	public partial class Configuration : IPluginConfiguration
	{
		[Notify] private Dictionary<string, string> persistence = new();
		[Notify] private int hasConfirmedReShadeVersion = -1;

		// Files
		[Notify] private string? lastSaveDirectory;
		[Notify] private SaveService.SaveConfiguration saveConfig = new();
		[Notify] private string? defaultAuthor;
		[Notify] private string? defaultVersion = "1.0";

		// Photos
		[Notify] private string? photoDirectory;
		[Notify] private PhotosService.Formats photoFormat = PhotosService.Formats.Jpeg;
		[Notify] private bool photoIncludeMetaData = true;
		[Notify] private bool photoCaptureDepth = false;

		[Notify] private bool photoAnimationFlash = true;
		[Notify] private bool photoAnimationPreview = true;

		// Analytics
		[Notify] private bool hasConfirmedAnalyticOptions = false;
		[Notify] private bool sendOptionalAnalytics = false;
		[Notify] private bool sendErrorReports = true;

		// Interface
		[Notify] private bool hideLauncherButton = false;
		[Notify] private bool openGroupPose = false;
		[Notify] private bool hideGenitals = true;
		[Notify] private bool enableGlobalOverlay = true;
		[Notify] private bool showOverlays = true;
		[Notify] private Dictionary<string, int> overlays = new();
		[Notify] private List<string> openPanels = new();
		[Notify] private Dictionary<string, Launcher.TaskBarEntrySave> minimizedTaskBarEntries = new();
		[Notify] private string theme = "Dark";
		[Notify] private string trimColor = "Pink";
		[Notify] private string launcher = "Default";
		[Notify] private bool isAioWindowOpen = false;

		// Input
		[Notify] private bool enableBinds = true;
		[Notify] private Dictionary<InputAction, List<Bind>> customBinds = new();

		// Library
		[Notify] private HashSet<string> favorites = new();
		[Notify] private PreviewModes libraryPreviewMode = PreviewModes.Permanent;

		// Scripts
		[Notify] private Dictionary<string, string> trustedScripts = new();

		public int Version { get; set; } = 0;

		public void Validate()
		{
			if (this.PhotoDirectory == null)
			{
				this.PhotoDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Studio Fourteen");
			}
		}
	}
}
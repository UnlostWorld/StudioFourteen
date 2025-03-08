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

namespace StudioFourteen.Launcher;

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.SPA;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
public partial class LauncherMenu : Control
{
	private Button? userButton;
	private Button? powerButton;
	private TextBox? searchBox;

	public LauncherMenu()
	{
		this.AddPanel<Marketplace.MarketplacePanel>("fa-Shop", "Marketplace", false);
		this.AddPanel<Library.LibraryWindow>("fa-Book", "Library");

		this.AddPanel<CameraPanel>("Camera", "Camera");
		this.AddPanel<EnvironmentPanel>("fa-CloudMoonRain", "Environment");
		this.AddPanel<Appearance.CharacterPanel>("fa-UserShield", "Character");
		this.AddPanel<Posing.PoseWindow>("fa-Running", "Pose");
		this.AddPanel<Library.LibraryWindow>("fa-Lightbulb", "Lighting", false);
		this.AddPanel<Library.LibraryWindow>("fa-Chair", "Furniture", false);
		this.AddPanel<Library.LibraryWindow>("fa-Users", "Crowds", false);
		this.AddPanel<Library.LibraryWindow>("fa-Fire", "Effects", false);

		this.AddPanel<Photos.PhotoWindow>("fa-Image", "Photo");
		this.AddEntry<SpaLauncherEntry>("fa-ObjectGroup", "Spa");

		this.AddPanel<History.HistoryPanel>("fa-History", "History");
		this.AddPanel<Save.SaveWindow>("fa-Save", "Save");
		this.AddPanel<Settings.SettingsPanel>("fa-Cogs", "Settings");
	}

	public FastObservableCollection<LauncherEntry> Entries { get; init; } = new();
	protected ServiceManager Services => ServiceManager.Instance;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.userButton = this.GetTemplateChild("PART_UserButton") as Button;
		this.powerButton = this.GetTemplateChild("PART_PowerButton") as Button;
		this.searchBox = this.GetTemplateChild("PART_SearchBox") as TextBox;

		if (this.powerButton != null)
		{
			this.powerButton.Click += this.OnPowerClicked;
		}
	}

	private void OnPowerClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.CloseStudio();
		this.IsOpen = false;
	}

	private void AddPanel<TPanel>(string icon, string name, bool enabled = true)
		where TPanel : Panel, new()
	{
		this.AddEntry<PanelLauncherEntry<TPanel>>(icon, name, enabled);
	}

	private void AddEntry<T>(string icon, string name, bool enabled = true)
		where T : LauncherEntry
	{
		T? entry = Activator.CreateInstance(typeof(T), [this]) as T;
		if (entry == null)
			return;

		entry.Name = name;
		entry.Icon = icon;
		entry.IsEnabled = enabled;
		this.Entries.Add(entry);
	}
}

public abstract class LauncherEntry(LauncherMenu menu)
	: ViewModel
{
	protected readonly LauncherMenu owner = menu;

	public bool IsEnabled { get; set; }
	public string? Icon { get; set; }
	public string? Name { get; set; }

	public string? DisplayName => Resources.Find($"LOC_{this.Name}", this.Name ?? string.Empty);
	public string? Description => Resources.Find($"LOC_{this.Name}Desc", this.Name ?? string.Empty);

	public bool IsOpen
	{
		get => this.GetIsOpen();
		set => this.Open();
	}

	protected abstract bool GetIsOpen();
	protected abstract void SetOpen();

	private void Open()
	{
		this.owner.IsOpen = false;
		Task.Run(() => this.SetOpen());
	}
}

public class PanelLauncherEntry<T> : LauncherEntry
	where T : Panel, new()
{
	public PanelLauncherEntry(LauncherMenu menu)
		: base(menu)
	{
		this.Services.Panels.PanelOpened += this.OnPanelChanged;
		this.Services.Panels.PanelClosed += this.OnPanelChanged;
		this.Services.Panels.PanelMinimized += this.OnPanelChanged;
	}

	protected override bool GetIsOpen() => this.Services.Panels.GetIsOpen<T>();
	protected override void SetOpen() => this.Services.Panels.SetIsOpen<T>(true);

	private void OnPanelChanged(Panel panel)
	{
		this.RaisePropertyChanged(nameof(LauncherEntry.IsOpen));
	}
}

public class SpaLauncherEntry(LauncherMenu menu)
	: LauncherEntry(menu)
{
	protected override bool GetIsOpen() => SpaWindow.GetIsOpen();
	protected override void SetOpen() => SpaWindow.OpenSpa();
}
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
using System.Windows;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.AIO;
using WpfUtils.Extensions;
using StudioFourteen.Environment;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<PanelContextBase>("Context")]
public partial class LauncherMenu : Control
{
	private Button? userButton;
	private Button? powerButton;
	private TextBox? searchBox;

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

	public PanelContextBase GetContext()
	{
		if (this.Context == null)
			throw new Exception("No Context in launcher menu");

		return this.Context;
	}

	partial void OnContextChanged()
	{
		if (this.Context == null)
			return;

		this.Populate();
	}

	private void Populate()
	{
		this.Entries.Clear();

		this.AddPanel<Marketplace.MarketplacePanel>("Marketplace", false);
		this.AddPanel<Library.LibraryWindow>("Library");

		this.AddPanel<CameraPanel>("Camera");
		this.AddPanel<EnvironmentPanel>("Environment");
		this.AddPanel<Appearance.CharacterPanel>("Character");
		this.AddPanel<Posing.PoseWindow>("Pose");
		this.AddPanel<Library.LibraryWindow>("Lighting", false);
		this.AddPanel<Library.LibraryWindow>("Furniture", false);
		this.AddPanel<Library.LibraryWindow>("Crowds", false);
		this.AddPanel<Library.LibraryWindow>("Effects", false);

		this.AddPanel<Library.LibraryWindow>("Animation", false);
		this.AddPanel<Library.LibraryWindow>("Sequencer", false);

		this.AddPanel<Photos.PhotoWindow>("Photo");

		if (this.Context is not AioPanelContext)
			this.AddEntry<AioLauncherEntry>("AIO");

		this.AddPanel<History.HistoryPanel>("History");
		this.AddPanel<Save.SaveWindow>("Save");
		this.AddPanel<Settings.SettingsPanel>("Settings");
	}

	private void OnPowerClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.CloseStudio();
		this.IsOpen = false;
	}

	private void AddPanel<TPanel>(string name, bool enabled = true)
		where TPanel : Panel, new()
	{
		this.AddEntry<PanelLauncherEntry<TPanel>>(name, enabled);
	}

	private void AddEntry<T>(string name, bool enabled = true)
		where T : LauncherEntry
	{
		T? entry = Activator.CreateInstance(typeof(T), [this]) as T;
		if (entry == null)
			return;

		entry.Name = name;
		entry.IsEnabled = enabled;
		this.Entries.Add(entry);
	}
}

public abstract class LauncherEntry(LauncherMenu menu)
	: ViewModel
{
	protected readonly LauncherMenu owner = menu;

	public bool IsEnabled { get; set; }
	public string? Name { get; set; }
	public bool IsAIO { get; set; }

	public string? DisplayName => Resources.Find($"LOC_{this.Name}", this.Name ?? string.Empty);
	public string? Description => Resources.Find($"LOC_{this.Name}Desc", this.Name ?? string.Empty);
	public object? Icon => Resources.Find($"ICON_Title_{this.Name}");

	public bool IsOpen
	{
		get => this.GetIsOpen();
		set => this.Open();
	}

	public PanelContextBase GetContext() => this.owner.GetContext();

	protected abstract bool GetIsOpen();
	protected abstract void SetOpen();

	private void Open()
	{
		this.owner.IsOpen = false;
		this.SetOpen();
	}
}

public class PanelLauncherEntry<T> : LauncherEntry
	where T : Panel, new()
{
	public PanelLauncherEntry(LauncherMenu menu)
		: base(menu)
	{
		this.GetContext().PanelOpened += this.OnPanelChanged;
		this.GetContext().PanelClosed += this.OnPanelChanged;
		this.GetContext().PanelMinimized += this.OnPanelChanged;
	}

	protected override bool GetIsOpen() => this.GetContext().GetOpenPanel<T>() != null;
	protected override void SetOpen() => this.GetContext().CreatePanel<T>(true);

	private void OnPanelChanged(Panel panel)
	{
		this.RaisePropertyChanged(nameof(LauncherEntry.IsOpen));
	}
}

public class AioLauncherEntry(LauncherMenu menu)
	: LauncherEntry(menu)
{
	protected override bool GetIsOpen() => AioWindow.GetIsOpen();
	protected override void SetOpen() => AioWindow.OpenAio();
}
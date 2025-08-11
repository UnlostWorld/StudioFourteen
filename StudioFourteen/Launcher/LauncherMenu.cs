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
using PropertyChanged.SourceGenerator;
using StudioFourteen.AIO;
using StudioFourteen.Environment;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Scene.Cameras;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Settings;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<PanelContextBase>("Context")]
public partial class LauncherMenu : Control
{
	[Notify] private bool isStudioOpen;
	[Notify] private bool isInGPose = false;
	[Notify] private bool isGPoseSettingsOpen = false;

	public LauncherMenu()
	{
		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
	}

	public FastObservableCollection<LauncherEntry> Entries { get; init; } = new();

	protected ServiceManager Services => ServiceManager.Instance;
	protected SettingsService.Configuration Configuration => this.Services.Settings.Current;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
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

		this.AddPanel<Marketplace.MarketplacePanel>(false);
		this.AddPanel<Library.LibraryPanel>();

		if (this.Configuration.EnableInspector)
			this.AddPanel<Selection.InspectorPanel>();

		if (this.Configuration.EnableDedicatedInspectors)
		{
			this.AddPanel<CameraPanel>();
			this.AddPanel<CharacterPanel>();
		}

		this.AddPanel<EnvironmentPanel>();
		this.AddPanel<Posing.PosePanel>();
		this.AddPanel<Animation.AnimationPanel>();
		this.AddPanel<Photos.PhotoPanel>();

		if (this.Configuration.AllInOne != SettingsService.Configuration.AioModes.Disabled
			&& this.Context is not AioPanelContext)
			this.AddEntry<AioLauncherEntry>();

		this.AddPanel<History.HistoryPanel>();
		this.AddPanel<Settings.SettingsPanel>();
	}

	private void OnPowerClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.CloseStudio();
		this.IsOpen = false;
	}

	private void AddPanel<TPanel>(bool enabled = true)
		where TPanel : Panel, new()
	{
		this.AddEntry<PanelLauncherEntry<TPanel>>(enabled);
	}

	private void AddEntry<T>(bool enabled = true)
		where T : LauncherEntry
	{
		T? entry = Activator.CreateInstance(typeof(T), [this]) as T;
		if (entry == null)
			return;

		entry.IsEnabled = enabled;
		this.Entries.Add(entry);
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.Opening += this.OnStudioStateChanged;
		this.Services.Studio.Closing += this.OnStudioStateChanged;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged += this.OnGroupPoseSettingsStateChanged;

		this.IsInGPose = this.Services.GroupPose.IsGroupPosing;
		this.IsGPoseSettingsOpen = this.Services.GroupPose.IsGroupPoseSettingsWindowVisible;

		this.OnStudioStateChanged();
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		this.Services.Studio.Opening -= this.OnStudioStateChanged;
		this.Services.Studio.Closing -= this.OnStudioStateChanged;
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		this.Services.GroupPose.SettingsStateChanged -= this.OnGroupPoseSettingsStateChanged;
	}

	private void OnStudioStateChanged()
	{
		this.Dispatcher.Invoke(() => this.IsStudioOpen = this.Services.Studio.IsOpen);
	}

	private void OnIsStudioOpenChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.Services.Studio.OpenStudio();
		}
		else
		{
			this.Services.Studio.CloseStudio();
		}
	}

	private void OnGroupPoseSettingsStateChanged(bool settingsState)
	{
		this.Dispatcher.Invoke(() => this.IsGPoseSettingsOpen = settingsState);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Dispatcher.Invoke(() => this.IsInGPose = newState);
	}

	private void OnIsInGPoseChanged(bool oldValue, bool newValue)
	{
		this.Services.GroupPose.SetGroupPose(newValue);
	}

	private void OnIsGPoseSettingsOpenChanged(bool oldValue, bool newValue)
	{
		this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(newValue);
	}
}

public abstract class LauncherEntry(LauncherMenu menu)
	: ViewModel
{
	protected readonly LauncherMenu owner = menu;

	public bool IsEnabled { get; set; }
	public bool IsAIO { get; set; }

	public abstract string? DisplayName { get; }
	public abstract string? Description { get; }
	public abstract object? Icon { get; }

	public bool IsOpen
	{
		get => this.IsEnabled && this.GetIsOpen();
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

	public override string? DisplayName => this.Services.Panels.GetPanelTitle(typeof(T));
	public override string? Description => this.Services.Panels.GetPanelDescription(typeof(T));
	public override object? Icon => this.Services.Panels.GetPanelIcon(typeof(T));

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
	public override string? DisplayName => Resources.Find($"LOC_AIO", "AIO");
	public override string? Description => Resources.Find($"LOC_AIODesc", string.Empty);
	public override object? Icon => Resources.Find($"ICON_Title_AIO");

	protected override bool GetIsOpen() => AioWindow.GetIsOpen();
	protected override void SetOpen() => AioWindow.OpenAio();
}
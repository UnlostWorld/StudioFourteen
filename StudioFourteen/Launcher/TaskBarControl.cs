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
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DependencyPropertyGenerator;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Icons;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Settings;
using WpfUtils;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

[DependencyProperty<bool>("IsOpen")]
[DependencyProperty<PanelContextBase>("Context")]
[DependencyProperty<bool>("HideBackground")]
public partial class TaskBarControl : Control
{
	private readonly Dictionary<Type, TaskBarEntry> panelEntries = new();

	public TaskBarControl()
	{
		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Studio.Closing += this.OnStudioClosing;

		this.Loaded += this.OnLoaded;
	}

	public FastObservableCollection<TaskBarEntry> Entries { get; init; } = new();

	protected ServiceManager Services => ServiceManager.Instance;
	protected SettingsService.Configuration Settings => this.Services.Settings.Current;

	public void AddEntry<T>()
	{
		TaskBarEntry entry = new(typeof(T));
		entry.IsMinimized = true;

		this.Dispatcher.Invoke(() =>
		{
			this.panelEntries.Add(typeof(T), entry);
			this.Entries.Add(entry);
		});
	}

	partial void OnContextChanged(PanelContextBase? oldValue, PanelContextBase? newValue)
	{
		if (oldValue != null)
		{
			oldValue.PanelOpened += this.OnPanelOpened;
			oldValue.PanelClosed += this.OnPanelClosed;
			oldValue.PanelMinimized += this.OnPanelMinimized;
			oldValue.PanelActivated += this.OnPanelActivated;
			oldValue.PanelDeactivated += this.OnPanelDeactivated;
		}

		if (newValue != null)
		{
			newValue.PanelOpened += this.OnPanelOpened;
			newValue.PanelClosed += this.OnPanelClosed;
			newValue.PanelMinimized += this.OnPanelMinimized;
			newValue.PanelActivated += this.OnPanelActivated;
			newValue.PanelDeactivated += this.OnPanelDeactivated;
		}
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.OnStudioOpening();
		}

		if (this.Context == this.Services.Panels.AioPanels)
		{
			this.IsOpen = true;
		}
	}

	private void OnStudioOpening()
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			if (this.Context == this.Services.Panels.GamePanels
				&& this.Settings.AllInOne != SettingsService.Configuration.AioModes.Always)
			{
				this.IsOpen = true;
			}
		});
	}

	private void OnStudioClosing()
	{
		this.Dispatcher.BeginInvoke(() =>
		{
			if (this.Context == this.Services.Panels.GamePanels
				&& this.Settings.AllInOne != SettingsService.Configuration.AioModes.Always)
			{
				this.IsOpen = false;
			}
		});
	}

	private void OnPanelOpened(Panel panel)
	{
		this.OnPanelOpenedAsync(panel).Run();
	}

	private async Task OnPanelOpenedAsync(Panel panel)
	{
		await this.MainThread();

		PanelContextBase? context = this.Context;
		if (panel.GetContext() != context)
			return;

		if (!panel.RememberWindowState)
			return;

		TaskBarEntry? entry;
		this.panelEntries.TryGetValue(panel.GetType(), out entry);

		if (entry == null)
		{
			entry = new(panel.GetType());
			this.panelEntries.Add(panel.GetType(), entry);

			this.Entries.Add(entry);
		}
		else
		{
			entry.IsMinimized = false;
		}
	}

	private void OnPanelClosed(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		this.panelEntries.Remove(panel.GetType());
		this.RemoveEntry(entry).Run();
	}

	private void OnPanelMinimized(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsMinimized = true;
	}

	private void OnPanelDeactivated(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsActive = false;
	}

	private void OnPanelActivated(Panel panel)
	{
		TaskBarEntry? entry;
		if (!this.panelEntries.TryGetValue(panel.GetType(), out entry) || entry == null)
			return;

		entry.IsActive = true;
	}

	private async Task RemoveEntry(TaskBarEntry entry)
	{
		await this.MainThread();
		entry.IsVisible = false;
		await Task.Delay(250);
		this.Entries.Remove(entry);
	}
}

public partial class TaskBarEntry : ViewModel
{
	private readonly Type panelType;

	[Notify] private bool isMinimized = false;
	[Notify] private bool isActive = true;
	[Notify] private bool isVisible = true;

	public TaskBarEntry(Type panelType)
	{
		this.panelType = panelType;
	}

	public Type? Type => this.panelType;
	public string Title => ServiceManager.Instance.Panels.GetPanelTitle(this.panelType);
	public string Description => ServiceManager.Instance.Panels.GetPanelDescription(this.panelType);
	public object? Icon => ServiceManager.Instance.Panels.GetPanelIcon(this.panelType);
}

[DependencyProperty<bool>("IsMinimized")]
[DependencyProperty<object>("Icon")]
[DependencyProperty<bool>("IsTaskVisible")]
[DependencyProperty<Type>("PanelType")]
[DependencyProperty<bool>("IsActive")]
public partial class TaskBarButtonControl : Control
{
	protected ServiceManager Services => ServiceManager.Instance;

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.ChangedButton == MouseButton.Left)
		{
			this.HandleClickAsync().Run();
		}
	}

	private async Task HandleClickAsync()
	{
		if (this.PanelType == null)
			return;

		TaskBarControl? taskBar = this.FindParent<TaskBarControl>();
		if (taskBar == null || taskBar.Context == null)
			return;

		await taskBar.Context.TogglePanel(this.PanelType);
	}
}
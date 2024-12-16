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

namespace StudioFourteen.Library.LibraryMenu;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using Serilog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Commands;
using WpfUtils.Controls;
using WpfUtils.Extensions;

public interface ILibraryContextMenu
{
	MenuEntry AddMenu(IconChar? icon, string? label, Action? invoke = null);
}

[DependencyProperty<LibraryEntryBase>("Entry")]
[DependencyProperty<bool>("IsExpanded")]
[DependencyProperty<object>("SourceHeader")]
[DependencyProperty<object>("SourceHeaderTemplate")]
[DependencyProperty<object>("BackgroundDetail")]
[DependencyProperty<Action<LibraryContextMenu>>("CollectingMenus")]
public partial class LibraryContextMenu : PopOut, ILibraryContextMenu
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();
	private readonly List<MenuEntry> pendingChildren = new();

	private UIElement? placementTarget;
	private LibraryEntryBase? currentEntry;

	public LibraryContextMenu()
	{
		this.MouseRightButtonUp += this.OnMouseRightButtonUp;
	}

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public void Enter(LibraryEntryBase entry, FrameworkElement placementTarget)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		if (this.IsOpen && this.currentEntry != entry)
			this.IsOpen = false;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.currentEntry = entry;
		this.IsExpanded = false;
		this.StaysOpen = true;

		this.ShowResultMenu().Run();
	}

	public void Leave(LibraryEntryBase? result)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsOpen = false;
	}

	public void Expand()
	{
		this.IsExpanded = true;

		this.CollectMenus().Run();

		this.Dispatcher.Invoke(() =>
		{
			this.Focus();
			this.IsHitTestVisible = true;
			this.StaysOpen = false;
		});
	}

	public void OnMenuInvoked(MenuEntry entry)
	{
		this.IsOpen = false;
	}

	public MenuEntry AddMenu(IconChar? icon, string? label, Action? invoke = null)
	{
		MenuEntry entry = new(icon, label, invoke);
		this.pendingChildren.Add(entry);
		return entry;
	}

	public void AddMenu(MenuEntry entry)
	{
		this.pendingChildren.Add(entry);
	}

	private async Task ShowResultMenu()
	{
		await this.MainThread();

		if (this.placementTarget == null || this.currentEntry == null)
			return;

		this.Entry = this.currentEntry;
		this.PlacementTarget = this.placementTarget;
		this.IsOpen = true;
	}

	private async Task CollectMenus()
	{
		this.Menus.Clear();

		this.CollectingMenus?.Invoke(this);

		if (this.Entry != null)
		{
			await this.Entry.GetLibraryMenus(this);
		}

		await this.MainThread();
		foreach(var entry in this.pendingChildren)
		{
			entry.SetContextMenu(this);
			this.Menus.Add(entry);
		}

		this.pendingChildren.Clear();
	}

	private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.IsExpanded)
		{
			this.Expand();
			e.Handled = true;
		}
	}
}

public class MenuEntry
{
	private readonly List<MenuEntry> pendingChildren = new();
	private readonly Action? invoke;

	public MenuEntry()
	{
	}

	public MenuEntry(IconChar? icon, string? label, Action? invoke = null)
	{
		this.Icon = icon;
		this.Label = label;
		this.invoke = invoke;
		this.OnClicked = new SimpleCommand(this.Invoke);
	}

	public IconChar? Icon { get; set; }
	public string? Label { get; set; }
	public ICommand? OnClicked { get; set; }

	public bool IsEnabled { get; set; } = true;

	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public LibraryContextMenu? ContextMenu { get; private set; }

	public bool HasChildren => this.Children.Count > 0 || this.pendingChildren.Count > 0;

	public void SetContextMenu(LibraryContextMenu? contextMenu)
	{
		this.ContextMenu = contextMenu;

		foreach (MenuEntry entry in this.pendingChildren)
		{
			this.Children.Add(entry);
		}

		this.pendingChildren.Clear();

		foreach (MenuEntry entry in this.Children)
		{
			entry.SetContextMenu(contextMenu);
		}
	}

	public void Invoke()
	{
		this.invoke?.Invoke();
		this.ContextMenu?.OnMenuInvoked(this);
	}

	public MenuEntry AddChild(IconChar? icon, string? label, Action? invoke = null)
	{
		MenuEntry child = new(icon, label, invoke);
		this.pendingChildren.Add(child);
		return child;
	}
}
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
using StudioFourteen.Library.Results;
using StudioFourteen.Library.Sources;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Commands;
using WpfUtils.Controls;
using WpfUtils.Extensions;
using WpfUtils.Utils;

public interface ILibraryContextMenu
{
	MenuEntry AddMenu(IconChar? icon, string? label, Action? invoke = null);
}

[DependencyProperty<LibraryEntryBase>("Entry")]
[DependencyProperty<bool>("IsExpanded")]
public partial class LibraryContextMenu : PopOut, ILibraryContextMenu
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();
	private readonly List<MenuEntry> pendingChildren = new();

	private UIElement? placementTarget;
	private Result? currentResult;

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public void Enter(Result result, FrameworkElement placementTarget)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		if (this.IsOpen && this.currentResult != result)
			this.IsOpen = false;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.currentResult = result;
		this.IsExpanded = false;
		this.StaysOpen = true;

		this.ShowResultMenu().Run();
	}

	public void Leave(Result? result)
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

	private async Task ShowResultMenu()
	{
		await this.MainThread();

		if (this.placementTarget == null || this.currentResult == null)
			return;

		this.Entry = this.currentResult.Entry;
		this.PlacementTarget = this.placementTarget;
		this.IsOpen = true;
		this.DataContext = this.currentResult.Entry;
	}

	private async Task CollectMenus()
	{
		this.Menus.Clear();

		if (this.Entry == null)
			return;

		await this.Entry.GetLibraryMenus(this);

		await this.MainThread();
		foreach(var entry in this.pendingChildren)
		{
			entry.SetContextMenu(this);
			this.Menus.Add(entry);
		}

		this.pendingChildren.Clear();
	}
}

public class MenuEntry(IconChar? icon, string? label, Action? invoke = null)
{
	private readonly List<MenuEntry> pendingChildren = new();

	public IconChar? Icon => icon;
	public bool IsEnabled { get; set; } = true;
	public ICommand? OnClicked => new SimpleCommand(this.Invoke);
	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public LibraryContextMenu? ContextMenu { get; private set; }
	public string? Label => label;

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
		invoke?.Invoke();
		this.ContextMenu?.OnMenuInvoked(this);
	}

	public MenuEntry AddChild(IconChar? icon, string? label, Action? invoke = null)
	{
		MenuEntry child = new(icon, label, invoke);
		this.pendingChildren.Add(child);
		return child;
	}
}
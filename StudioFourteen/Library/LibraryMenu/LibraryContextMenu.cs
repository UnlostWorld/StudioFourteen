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

[DependencyProperty<LibraryEntryBase>("Entry")]
[DependencyProperty<bool>("IsExpanded")]
public partial class LibraryContextMenu : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();

	private readonly FuncQueue openQueue;
	private UIElement? placementTarget;
	private Result? currentResult;

	public LibraryContextMenu()
	{
		this.openQueue = new(this.ShowResultMenu, 250);
	}

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public void Enter(Result result, FrameworkElement placementTarget)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.currentResult = result;
		this.IsExpanded = false;
		this.StaysOpen = true;
		this.openQueue.Invoke();
	}

	public void Leave(Result result)
	{
		if (this.currentResult == result)
		{
			this.openQueue.Cancel();
		}

		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsOpen = false;
	}

	public void Expand()
	{
		this.openQueue.InvokeImmediate();

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

		List<MenuEntry> menus = await this.Entry.GetLibraryMenus();
		foreach(MenuEntry entry in menus)
		{
			entry.SetContextMenu(this);
		}

		await this.MainThread();
		this.Menus.Replace(menus);
	}
}

public class MenuEntry(IconChar? icon, string? label, Action? invoke = null)
{
	public IconChar? Icon => icon;
	public bool IsEnabled => true;
	public ICommand? OnClicked => new SimpleCommand(this.Invoke);
	public FastObservableCollection<MenuEntry> Children { get; init; } = new();
	public LibraryContextMenu? ContextMenu { get; private set; }
	public string? Label => label;

	public void SetContextMenu(LibraryContextMenu? contextMenu)
	{
		this.ContextMenu = contextMenu;

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
}
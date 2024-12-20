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
using FFXIVClientStructs.FFXIV.Client.Game.UI;
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
	MenuEntry AddMenu(IconChar? icon, string? label);
	MenuEntry AddMenu(IconChar? icon, string? label, Func<Task> invoke);
	MenuEntry AddMenu(IconChar? icon, string? label, Action invoke);
}

[DependencyProperty<LibraryEntryBase>("Entry")]
[DependencyProperty<string>("MultiSelectLabel")]
[DependencyProperty<bool>("IsExpanded")]
[DependencyProperty<object>("SourceHeader")]
[DependencyProperty<object>("SourceHeaderTemplate")]
[DependencyProperty<object>("BackgroundDetail")]
[DependencyProperty<Action<LibraryContextMenu>>("CollectingMenus")]
public partial class LibraryContextMenu : PopOut
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();
	private readonly List<LibraryEntryBase> currentEntries = new();
	private UIElement? placementTarget;

	public LibraryContextMenu()
	{
		this.MouseRightButtonUp += this.OnMouseRightButtonUp;
	}

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

	public bool Enter(FrameworkElement placementTarget)
	{
		if (this.IsOpen && this.IsExpanded)
			return false;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.IsExpanded = false;
		this.StaysOpen = true;

		this.ShowResultMenu().Run();
		return true;
	}

	public void Enter(List<LibraryEntryBase> entries, FrameworkElement placementTarget)
	{
		this.MultiSelectLabel = $"{entries.Count} items";
		this.currentEntries.Clear();
		this.currentEntries.AddRange(entries);

		this.Enter(placementTarget);
	}

	public void Enter(LibraryEntryBase entry, FrameworkElement placementTarget)
	{
		if (this.IsOpen && !this.currentEntries.Contains(entry))
			this.IsOpen = false;

		this.MultiSelectLabel = null;
		this.currentEntries.Clear();
		this.currentEntries.Add(entry);

		this.Enter(placementTarget);
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

	private async Task ShowResultMenu()
	{
		await this.MainThread();

		if (this.placementTarget == null || this.currentEntries.Count <= 0)
			return;

		this.Entry = null;
		if (this.currentEntries.Count == 1)
			this.Entry = this.currentEntries[0];

		this.PlacementTarget = this.placementTarget;
		this.IsOpen = true;
	}

	private async Task CollectMenus()
	{
		this.Menus.Clear();

		this.CollectingMenus?.Invoke(this);

		List<ContextMenuRoot> roots = new();

		foreach(LibraryEntryBase entry in this.currentEntries)
		{
			ContextMenuRoot root = new();
			await entry.GetLibraryMenus(root);
			roots.Add(root);
		}

		await this.MainThread();

		if (this.currentEntries.Count == 1)
		{
			foreach (MenuEntry entry in roots[0].Children)
			{
				entry.SetContextMenu(this);
				this.Menus.Add(entry);
			}
		}
		else if (this.currentEntries.Count > 1)
		{
			// Get all entries, grouped by their labels.
			Dictionary<string, List<MenuEntry>> entryLookup = new();
			List<string> entryLabels = new();
			foreach (ContextMenuRoot root in roots)
			{
				foreach (MenuEntry entry in root.Children)
				{
					if (entry.Label == null)
						continue;

					if (!entry.IsEnabled)
						continue;

					if (!entryLookup.ContainsKey(entry.Label))
					{
						entryLabels.Add(entry.Label);
						entryLookup.Add(entry.Label, new());
					}

					entryLookup[entry.Label].Add(entry);
				}
			}

			// get all menu entries that are valid for all selected library entries
			foreach (string label in entryLabels)
			{
				if (entryLookup[label].Count != this.currentEntries.Count)
					continue;

				Func<Task> invoke = async () =>
				{
					foreach (MenuEntry subEntry in entryLookup[label])
					{
						await subEntry.Invoke();
					}
				};

				MenuEntry groupEntry = new(entryLookup[label][0].Icon, label, invoke);
				groupEntry.SetContextMenu(this);
				this.Menus.Add(groupEntry);
			}
		}
	}

	private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (!this.IsExpanded)
		{
			this.Expand();
			e.Handled = true;
		}
	}

	private class ContextMenuRoot : ILibraryContextMenu
	{
		public readonly List<MenuEntry> Children = new();

		public void AddMenu(MenuEntry entry)
		{
			this.Children.Add(entry);
		}

		public MenuEntry AddMenu(IconChar? icon, string? label)
		{
			MenuEntry entry = new(icon, label);
			this.Children.Add(entry);
			return entry;
		}

		public MenuEntry AddMenu(IconChar? icon, string? label, Action invoke)
		{
			Func<Task> f = () =>
			{
				invoke?.Invoke();
				return Task.CompletedTask;
			};

			return this.AddMenu(icon, label, f);
		}

		public MenuEntry AddMenu(IconChar? icon, string? label, Func<Task> invoke)
		{
			MenuEntry entry = new(icon, label, invoke);
			this.Children.Add(entry);
			return entry;
		}
	}
}

public class MenuEntry
{
	private readonly List<MenuEntry> pendingChildren = new();
	private readonly Func<Task>? invoke;

	public MenuEntry()
	{
	}

	public MenuEntry(IconChar? icon, string? label, Func<Task>? invoke = null)
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

	public Task Invoke()
	{
		Task? t = this.invoke?.Invoke();
		this.ContextMenu?.OnMenuInvoked(this);

		if (t == null)
			return Task.CompletedTask;

		return t;
	}

	public MenuEntry AddChild(IconChar? icon, string? label, Func<Task>? invoke = null)
	{
		MenuEntry child = new(icon, label, invoke);
		this.pendingChildren.Add(child);
		return child;
	}

	public MenuEntry AddChild(IconChar? icon, string? label, Action? invoke = null)
	{
		Func<Task> f = () =>
		{
			invoke?.Invoke();
			return Task.CompletedTask;
		};

		MenuEntry child = new(icon, label, f);
		this.pendingChildren.Add(child);
		return child;
	}
}
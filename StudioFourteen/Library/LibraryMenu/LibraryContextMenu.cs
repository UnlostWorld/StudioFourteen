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
using Serilog;
using StudioFourteen.Context;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Controls;
using WpfUtils.Extensions;
using WpfUtils.Utils;

[DependencyProperty<LibraryEntryBase>("Entry")]
[DependencyProperty<string>("MultiSelectLabel")]
[DependencyProperty<bool>("IsExpanded")]
[DependencyProperty<object>("SourceHeader")]
[DependencyProperty<object>("SourceHeaderTemplate")]
[DependencyProperty<object>("BackgroundDetail")]
[DependencyProperty<Action<LibraryContextMenu>>("CollectingMenus")]
public partial class LibraryContextMenu : PopOut, IContextMenu
{
	protected readonly ILogger Log = Logging.ForContext<LibraryContextMenu>();
	private readonly List<LibraryEntryBase> currentEntries = new();
	private readonly FuncQueue closeQueue;
	private UIElement? placementTarget;

	public LibraryContextMenu()
	{
		this.MouseRightButtonUp += this.OnMouseRightButtonUp;
		this.closeQueue = new(this.CloseActual, 200);
	}

	public ServiceManager Services => ServiceManager.Instance;
	public FastObservableCollection<MenuEntry> Menus { get; init; } = new();

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
		{
			this.closeQueue.Invoke();
		}
		else if (this.IsOpen && this.currentEntries.Contains(entry))
		{
			return;
		}

		this.MultiSelectLabel = null;
		this.currentEntries.Clear();
		this.currentEntries.Add(entry);

		this.Enter(placementTarget);
	}

	public void Leave(LibraryEntryBase? result)
	{
		if (this.IsOpen && this.IsExpanded)
			return;

		this.closeQueue.Invoke();
	}

	public void Expand()
	{
		this.IsExpanded = true;
		this.IsEnabled = true;

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
		this.Dispatcher.Invoke(() => this.IsOpen = false);
	}

	public void Enter(FrameworkElement placementTarget)
	{
		this.closeQueue.Cancel();

		if (this.IsOpen && this.IsExpanded)
			return;

		this.IsHitTestVisible = false;
		this.placementTarget = placementTarget;
		this.IsExpanded = false;
		this.IsEnabled = false;
		this.StaysOpen = true;

		if (this.placementTarget == null || this.currentEntries.Count <= 0)
			return;

		this.Entry = null;
		if (this.currentEntries.Count == 1)
			this.Entry = this.currentEntries[0];

		this.PlacementTarget = this.placementTarget;

		// Bump the pffset to get an already open panel to move.
		this.HorizontalOffset++;
		this.HorizontalOffset--;

		this.IsOpen = true;
	}

	private async Task CloseActual()
	{
		await this.MainThread();
		this.IsOpen = false;
	}

	private Task CollectMenus()
	{
		this.Menus.Clear();

		this.CollectingMenus?.Invoke(this);

		/*foreach (LibraryEntryBase entry in this.currentEntries)
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
					LongTaskWindow? ltw = await LongTaskWindow.Show();

					int count = entryLookup[label].Count;
					int current = 0;
					foreach (MenuEntry subEntry in entryLookup[label])
					{
						current++;
						ltw?.SetStatus($"{current} / {count}");

						await subEntry.Invoke();
					}

					ltw?.Close();
				};

				MenuEntry groupEntry = new(entryLookup[label][0].Icon, label, invoke);
				groupEntry.SetContextMenu(this);
				this.Menus.Add(groupEntry);
			}
		}*/

		return Task.CompletedTask;
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
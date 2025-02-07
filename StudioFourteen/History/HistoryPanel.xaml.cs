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

namespace StudioFourteen.History;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.Extensions;

using Panel = StudioFourteen.Panels.Panel;

public partial class HistoryPanel : Panel
{
	private bool isRefreshing;
	[Notify] private int selectedIndex;

	public FastObservableCollection<HistoryEntry> History { get; init; } = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.History.HistoryAdded += this.OnHistoryAdded;
		this.Services.History.HistoryRemoved += this.OnHistoryRemoved;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.History.HistoryAdded -= this.OnHistoryAdded;
		this.Services.History.HistoryRemoved -= this.OnHistoryRemoved;
	}

	private void OnHistoryAdded(Operation operation)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Refresh();
		});
	}

	private void OnHistoryRemoved(Operation operation)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Refresh();
		});
	}

	private void OnUndoClicked(object sender, RoutedEventArgs e)
	{
		this.Services.History.Undo();
	}

	private void OnRedoClicked(object sender, RoutedEventArgs e)
	{
		this.Services.History.Redo();
	}

	private void Refresh()
	{
		this.isRefreshing = true;
		List<HistoryEntry> history = new();
		HistoryEntry? mid = null;
		foreach (Operation operation in this.Services.History.UndoStack)
		{
			HistoryEntry entry = new();
			entry.Operation = operation;
			entry.IsPast = true;
			history.Add(entry);

			mid = entry;
		}

		history.Reverse();

		foreach (Operation operation in this.Services.History.RedoStack)
		{
			HistoryEntry entry = new();
			entry.Operation = operation;
			entry.IsPast = false;
			history.Add(entry);
		}

		this.History.Replace(history);
		this.SelectedIndex = -1;

		if (this.Services.History.UndoStack.Count > 0)
		{
			this.HistoryList.ScrollIntoView(mid);
		}

		this.isRefreshing = false;
	}

	private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this.isRefreshing)
			return;

		HistoryEntry? entry = this.HistoryList.SelectedItem as HistoryEntry;
		if (entry == null || entry.Operation == null)
			return;

		this.Services.History.GoTo(entry.Operation);
	}

	public class HistoryEntry
	{
		public Operation? Operation { get; set; }
		public bool IsPast { get; set; }
	}
}
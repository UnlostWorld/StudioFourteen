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
using WpfUtils.Extensions;

public partial class HistoryPanel : Panel
{
	[Notify] private int selectedIndex;
	public FastObservableCollection<OperationBase> History { get; init; } = new();

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

	private void OnHistoryAdded(OperationBase operation)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Refresh();
		});
	}

	private void OnHistoryRemoved(OperationBase operation)
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
		List<OperationBase> operations = new();

		foreach (OperationBase operation in this.Services.History.UndoStack)
		{
			operations.Add(operation);
		}

		operations.Reverse();

		foreach (OperationBase operation in this.Services.History.RedoStack)
		{
			operations.Add(operation);
		}

		this.History.Replace(operations);

		if (this.Services.History.RedoStack.Count <= 0)
		{
			this.SelectedIndex = -1;
		}
		else
		{
			this.SelectedIndex = this.Services.History.UndoStack.Count;
		}

		if (this.Services.History.UndoStack.Count > 0)
		{
			this.HistoryList.ScrollIntoView(this.Services.History.UndoStack.Peek());
		}
	}
}
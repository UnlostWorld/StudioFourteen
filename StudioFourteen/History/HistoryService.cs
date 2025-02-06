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

using FontAwesome.Sharp;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;

public interface IHistoryProvider
{
	OperationBase StartRecord();
	bool StopRecord(ref OperationBase operation);
}

public class HistoryService : ServiceBase
{
	private OperationBase? currentOperation;
	private IHistoryProvider? currentProvider;

	public delegate void HistoryEvent(OperationBase operation);
	public event HistoryEvent? HistoryAdded;
	public event HistoryEvent? HistoryRemoved;

	public Stack<OperationBase> UndoStack { get; init; } = new();
	public Stack<OperationBase> RedoStack { get; init; } = new();

	public bool CanUndo => this.UndoStack.Count > 0;
	public bool CanRedo => this.RedoStack.Count > 0;

	public void StartRecord(IHistoryProvider provider)
	{
		if (this.currentProvider != null || this.currentOperation != null)
			throw new Exception("Attempt to start histroy record while another record is in progress");

		this.currentProvider = provider;
		this.currentOperation = provider.StartRecord();
	}

	public void StopRecord(IHistoryProvider provider)
	{
		if (provider != this.currentProvider)
			throw new Exception("Attempt to stop histroy record while a different record is in progress");

		if (this.currentOperation == null)
			throw new Exception("Attempt to stop histroy record while no record is in progress");

		this.RedoStack.Clear();

		this.currentProvider = null;
		provider.StopRecord(ref this.currentOperation);

		this.UndoStack.Push(this.currentOperation);
		this.HistoryAdded?.Invoke(this.currentOperation);

		this.currentOperation = null;

		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
	}

	public void GoTo(OperationBase operation)
	{
		if (this.currentOperation != null && this.currentProvider != null)
			throw new Exception("Attempt to go to history while a record is in progress");

		if (this.UndoStack.Contains(operation))
		{
			// go undo
			OperationBase? reverseOperation = null;
			while(this.UndoStack.Count > 0 && reverseOperation != operation)
			{
				reverseOperation = this.UndoStack.Pop();
				reverseOperation.Revert();
				this.RedoStack.Push(reverseOperation);

				this.HistoryRemoved?.Invoke(reverseOperation);
			}

			this.RaisePropertyChanged(nameof(this.CanUndo));
			this.RaisePropertyChanged(nameof(this.CanRedo));
		}
		else if (this.RedoStack.Contains(operation))
		{
			// go redo
			OperationBase? forwardOperation = null;
			while (this.RedoStack.Count > 0 && forwardOperation != operation)
			{
				forwardOperation = this.RedoStack.Pop();
				forwardOperation.Apply();
				this.UndoStack.Push(forwardOperation);

				this.HistoryAdded?.Invoke(forwardOperation);
			}

			this.RaisePropertyChanged(nameof(this.CanUndo));
			this.RaisePropertyChanged(nameof(this.CanRedo));
		}
		else
		{
			throw new Exception("Specified operation is not part of the  history stacks");
		}
	}

	public void Undo()
	{
		if (!this.CanUndo)
			return;

		if (this.currentOperation != null && this.currentProvider != null)
			throw new Exception("Attempt to perform undo while a record is in progress");

		OperationBase reverseOperation = this.UndoStack.Pop();
		reverseOperation.Revert();
		this.RedoStack.Push(reverseOperation);

		this.HistoryRemoved?.Invoke(reverseOperation);
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
	}

	public void Redo()
	{
		if (!this.CanRedo)
			return;

		if (this.currentOperation != null && this.currentProvider != null)
			throw new Exception("Attempt to perform undo while a record is in progress");

		OperationBase forwardOperation = this.RedoStack.Pop();
		forwardOperation.Apply();
		this.UndoStack.Push(forwardOperation);

		this.HistoryAdded?.Invoke(forwardOperation);
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
	}
}

public abstract class OperationBase
{
	public abstract IconChar Icon { get; }
	public abstract string Description { get; }

	public abstract bool Apply();
	public abstract bool Revert();
}

public abstract class OperationCollectionBase : OperationBase
{
	protected readonly List<OperationBase> Children = new();

	public override bool Apply()
	{
		bool success = true;
		for(int i = 0; i < this.Children.Count; i++)
		{
			success &= this.Children[i].Apply();
		}

		return success;
	}

	public override bool Revert()
	{
		bool success = true;
		for (int i = this.Children.Count - 1; i >= 0; i--)
		{
			success &= this.Children[i].Revert();
		}

		return success;
	}

	public void AddChild(OperationBase child)
	{
		this.Children.Add(child);
	}
}

public abstract class CharacterOperationBase : OperationBase
{
	public int CharacterIndex { get; set; }
}
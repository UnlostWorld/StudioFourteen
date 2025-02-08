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
using Serilog;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;
using WpfUtils.Utils;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkComponentNumericInput.Delegates;

public interface IHistoryTarget
{
	string Name { get; }
	IconChar Icon { get; }
	bool IsReady { get; }

	Operation CreateHistoryOperation();
	void FinalizeHistoryOperation(ref Operation operation);
}

public class HistoryService : ServiceBase
{
	private readonly FuncQueue stopRecordQueue;
	private Operation? currentOperation;
	private IHistoryTarget? currentTarget;
	private bool isApplyingOperation = false;

	public HistoryService()
	{
		this.stopRecordQueue = new(this.PostChange, 500);
	}

	public delegate void HistoryEvent(Operation operation);
	public event HistoryEvent? HistoryAdded;
	public event HistoryEvent? HistoryRemoved;

	public Stack<Operation> UndoStack { get; init; } = new();
	public Stack<Operation> RedoStack { get; init; } = new();

	public bool CanUndo => this.UndoStack.Count > 0 && !this.isApplyingOperation;
	public bool CanRedo => this.RedoStack.Count > 0 && !this.isApplyingOperation;

	public bool IsApplyingOperation => this.isApplyingOperation;

	public static void Record(IHistoryTarget target, string description)
	{
		if (ServiceManager.ShutdownRequested)
			return;

		ServiceManager.Instance.History.RecordChange(target, description);
	}

	public void GoTo(Operation operation)
	{
		if (this.isApplyingOperation)
			throw new Exception("Attempt to go to history while a history operation is in progress");

		this.GoToAsync(operation).Run();
	}

	public async Task GoToAsync(Operation operation)
	{
		if (this.currentOperation != null)
			throw new Exception("Attempt to go to history while a record is in progress");

		if (this.isApplyingOperation)
			throw new Exception("Attempt to go to history while a history operation is in progress");

		this.isApplyingOperation = true;
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
		this.RaisePropertyChanged(nameof(this.IsApplyingOperation));

		if (this.UndoStack.Contains(operation))
		{
			// go undo
			Operation? reverseOperation = null;
			while(this.UndoStack.Count > 0 && reverseOperation != operation)
			{
				reverseOperation = this.UndoStack.Pop();
				await reverseOperation.Apply(true);
				this.RedoStack.Push(reverseOperation);

				this.HistoryRemoved?.Invoke(reverseOperation);
			}
		}
		else if (this.RedoStack.Contains(operation))
		{
			// go redo
			Operation? forwardOperation = null;
			while (this.RedoStack.Count > 0 && forwardOperation != operation)
			{
				forwardOperation = this.RedoStack.Pop();
				await forwardOperation.Apply(false);
				this.UndoStack.Push(forwardOperation);

				this.HistoryAdded?.Invoke(forwardOperation);
			}
		}
		else
		{
			throw new Exception("Specified operation is not part of the  history stacks");
		}

		this.isApplyingOperation = false;
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
		this.RaisePropertyChanged(nameof(this.IsApplyingOperation));
	}

	public void Undo()
	{
		this.UndoAsync().Run();
	}

	public async Task UndoAsync()
	{
		if (!this.CanUndo)
			return;

		if (this.stopRecordQueue.Pending)
			this.stopRecordQueue.InvokeImmediate();

		Operation reverseOperation = this.UndoStack.Pop();
		this.isApplyingOperation = true;
		await reverseOperation.Apply(true);
		this.isApplyingOperation = false;
		this.RedoStack.Push(reverseOperation);

		this.HistoryRemoved?.Invoke(reverseOperation);
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
		this.RaisePropertyChanged(nameof(this.IsApplyingOperation));
	}

	public void Redo()
	{
		this.RedoAsync().Run();
	}

	public async Task RedoAsync()
	{
		if (!this.CanRedo)
			return;

		if (this.stopRecordQueue.Pending)
			this.stopRecordQueue.InvokeImmediate();

		Operation forwardOperation = this.RedoStack.Pop();
		this.isApplyingOperation = true;
		await forwardOperation.Apply(false);
		this.isApplyingOperation = false;
		this.UndoStack.Push(forwardOperation);

		this.HistoryAdded?.Invoke(forwardOperation);
		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
		this.RaisePropertyChanged(nameof(this.IsApplyingOperation));
	}

	public void RecordChange(IHistoryTarget target, string description)
	{
		if (this.isApplyingOperation)
			return;

		if (this.currentOperation == null || this.currentTarget != target)
		{
			if (this.stopRecordQueue.Pending)
				this.stopRecordQueue.InvokeImmediate();

			this.currentTarget = target;
			this.currentOperation = target.CreateHistoryOperation();
			this.currentOperation.Description = description;
			this.currentOperation.StartRecord();
		}

		this.stopRecordQueue.Invoke();
	}

	private void PostChange()
	{
		if (this.currentOperation == null || this.currentTarget == null)
			throw new Exception("Attempt to stop histroy record while no record is in progress");

		bool didChange = this.currentOperation.EndRecord();

		if (!didChange)
			return;

		this.currentTarget.FinalizeHistoryOperation(ref this.currentOperation);

		// Is this operation already in the undo stack?
		// This can occur if the PostChange method is called multiple times,
		// which is valid.
		if (this.UndoStack.Count > 0 && this.currentOperation == this.UndoStack.Peek())
			return;

		this.RedoStack.Clear();

		this.UndoStack.Push(this.currentOperation);
		this.HistoryAdded?.Invoke(this.currentOperation);

		this.currentOperation = null;

		this.RaisePropertyChanged(nameof(this.CanUndo));
		this.RaisePropertyChanged(nameof(this.CanRedo));
	}
}

public abstract class Operation
{
	protected readonly ILogger Log = Logging.ForContext<Operation>();

	public IconChar Icon { get; set; }
	public string? TargetName { get; set; }
	public string? Description { get; set; }

	public Dictionary<string, object?> StartValues { get; init; } = new();
	public Dictionary<string, object?> EndValues { get; init; } = new();

	public abstract IHistoryTarget GetTarget();
	public abstract bool IsTarget(IHistoryTarget target);

	public void StartRecord()
	{
		IHistoryTarget target = this.GetTarget();
		this.Icon = target.Icon;
		this.TargetName = target.Name;
		PropertyInfo[] properties = target.GetType().GetProperties();
		foreach (PropertyInfo property in properties)
		{
			HistoryAttribute? attribute = property.GetCustomAttribute<HistoryAttribute>();
			if (attribute == null)
				continue;

			this.StartValues[property.Name] = property.GetValue(target);
		}

		MethodInfo[] methods = target.GetType().GetMethods();
		foreach(MethodInfo method in methods)
		{
			HistoryAttribute? attribute = method.GetCustomAttribute<HistoryAttribute>();
			if (attribute == null)
				continue;

			if (!method.Name.StartsWith("Get") || method.ReturnType == typeof(void))
				continue;

			this.StartValues[method.Name.Substring(3)] = method.Invoke(target, null);
		}
	}

	public bool EndRecord()
	{
		IHistoryTarget target = this.GetTarget();

		bool change = false;
		PropertyInfo[] properties = target.GetType().GetProperties();
		foreach (PropertyInfo property in properties)
		{
			HistoryAttribute? attribute = property.GetCustomAttribute<HistoryAttribute>();
			if (attribute == null)
				continue;

			if (!this.StartValues.ContainsKey(property.Name))
				continue;

			object? startValue = this.StartValues[property.Name];
			object? endValue = property.GetValue(target);
			change = !object.Equals(startValue, endValue);

			if (change)
			{
				this.EndValues[property.Name] = endValue;
			}
		}

		MethodInfo[] methods = target.GetType().GetMethods();
		foreach (MethodInfo method in methods)
		{
			HistoryAttribute? attribute = method.GetCustomAttribute<HistoryAttribute>();
			if (attribute == null)
				continue;

			if (!method.Name.StartsWith("Get") || method.ReturnType == typeof(void))
				continue;

			string name = method.Name.Substring(3);

			object? startValue = this.StartValues[name];
			object? endValue = method.Invoke(target, null);
			change = !object.Equals(startValue, endValue);

			if (change)
			{
				this.EndValues[name] = endValue;
			}
		}

		return change;
	}

	public async Task Apply(bool revert)
	{
		IHistoryTarget target = this.GetTarget();

		while (!target.IsReady)
			await Task.Delay(10);

		if (target.Name != this.TargetName)
			throw new Exception($"History operation target name mismatch. Expected {this.TargetName}, got {target.Name}");

		foreach ((string propertyName, object? value) in this.EndValues)
		{
			object? destValue = revert ? this.StartValues[propertyName] : value;

			PropertyInfo? property = target.GetType().GetProperty(propertyName);
			property?.SetValue(target, destValue);
		}

		foreach ((string propertyName, object? value) in this.EndValues)
		{
			object? destValue = revert ? this.StartValues[propertyName] : value;

			MethodInfo? method = target.GetType().GetMethod($"Set{propertyName}");
			if (method == null)
				continue;

			HistoryAttribute? attribute = method.GetCustomAttribute<HistoryAttribute>();
			if (attribute == null)
				continue;

			object? returnValue = method.Invoke(target, [destValue]);

			if (returnValue == null)
			{
			}
			else if (returnValue is Task task)
			{
				await task;
			}
			else
			{
				throw new Exception($"Unsupported return value in history operation: {returnValue.GetType()}");
			}
		}
	}
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
public class HistoryAttribute : Attribute
{
}
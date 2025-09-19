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

namespace StudioFourteen;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

public class SimpleCommand : ICommand, INotifyPropertyChanged
{
	protected Action? action;
	protected Func<Task>? asyncFunc;
	protected Func<bool>? canExecute;
	private bool isExecuting = false;
	private EventHandler? canExecuteChanged;

	public SimpleCommand()
	{
		CommandManager.RequerySuggested += this.OnRequerySuggested;
	}

	public SimpleCommand(Action action, Func<bool> canExecute)
		: this()
	{
		this.action = action;
		this.canExecute = canExecute;
	}

	public SimpleCommand(Action action)
		: this()
	{
		this.action = action;
	}

	public SimpleCommand(Func<Task> func)
		: this()
	{
		this.asyncFunc = func;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public event EventHandler? CanExecuteChanged
	{
		add
		{
			// yuck
			Dispatcher currentDispatcher = Dispatcher.CurrentDispatcher;
			this.canExecuteChanged += (s, e) =>
			{
				currentDispatcher?.BeginInvoke(() =>
				{
					value?.Invoke(s, e);
				});
			};
		}

		remove
		{
			this.canExecuteChanged -= value;
		}
	}

	public bool CanExecute(object? parameter)
	{
		if (this.isExecuting)
			return false;

		return this.CanExecute();
	}

	public void Execute(object? parameter)
	{
		this.isExecuting = true;
		this.RaiseCanExecuteChanged();

		this.action?.Invoke();

		Task.Run(async () =>
		{
			try
			{
				if (this.asyncFunc != null)
					await this.asyncFunc.Invoke();

				await this.Execute();
			}
			finally
			{
				this.isExecuting = false;
				this.RaiseCanExecuteChanged();
			}
		});
	}

	protected void RaiseCanExecuteChanged()
	{
		this.canExecuteChanged?.Invoke(this, new());
	}

	protected virtual bool CanExecute()
	{
		return this.canExecute?.Invoke() ?? true;
	}

	protected virtual void NotifyPropertyChanged(string propertyName)
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	protected virtual Task Execute()
	{
		return Task.CompletedTask;
	}

	private void OnRequerySuggested(object? sender, EventArgs e)
	{
		this.canExecuteChanged?.Invoke(sender, e);
	}
}
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
using System.Threading.Tasks;

public class DelayedFunction
{
	private readonly Func<Task> func;
	private int currentDelayValue;
	private Task? task;
	private bool isCanceling;

	public DelayedFunction(Func<Task> func, int delay)
	{
		this.Delay = delay;
		this.func = func;
	}

	public DelayedFunction(Action func, int delay)
	{
		this.Delay = delay;
		this.func = () =>
		{
			func.Invoke();
			return Task.CompletedTask;
		};
	}

	public int Delay { get; set; }
	public bool Pending { get; private set; }
	public bool Executing { get; private set; }

	public async Task WaitForPendingExecute()
	{
		this.ClearDelay();

		while (this.Pending || this.Executing)
		{
			await Task.Delay(1);
		}
	}

	public void Invoke()
	{
		this.currentDelayValue = this.Delay;
		this.isCanceling = false;

		if (this.task == null || this.task.IsCompleted)
		{
			this.task = Task.Run(this.RunTask);
		}
	}

	public void ClearDelay()
	{
		this.currentDelayValue = 0;
	}

	public void Cancel()
	{
		this.ClearDelay();
		this.isCanceling = true;
	}

	public void InvokeImmediate()
	{
		this.Invoke();
		this.ClearDelay();
	}

	private async Task RunTask()
	{
		// Double loops to handle case where a refresh delay was added
		// while the refresh was running
		while (this.currentDelayValue >= 0)
		{
			lock (this)
				this.Pending = true;

			while (this.currentDelayValue > 0)
			{
				await Task.Delay(10);
				this.currentDelayValue -= 10;
			}

			if (this.isCanceling)
			{
				this.currentDelayValue -= 1;
				this.isCanceling = false;
				return;
			}

			lock (this)
			{
				this.Executing = true;
				this.Pending = false;
			}

			try
			{
				await this.func.Invoke();
			}
			catch (Exception ex)
			{
				Studio.Log.Error(ex, "Error invoking delayed function");
			}

			this.currentDelayValue -= 1;

			lock (this)
			{
				this.Executing = false;
			}
		}
	}
}

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

namespace StudioFourteen.Services.Xivalonia.Platform;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using Avalonia.Threading;

public partial class DispatcherImpl : IControlledDispatcherImpl
{
	private readonly Stopwatch clock = Stopwatch.StartNew();
	private readonly Stopwatch timer = new Stopwatch();

	private Thread? uiThread;
	private bool isSignal = false;
	private long? timerMs = null;

	public DispatcherImpl()
	{
		this.uiThread = Thread.CurrentThread;
	}

	public event Action? Signaled;
	public event Action? Timer;

	public bool CurrentThreadIsLoopThread
	{
		get
		{
			if (this.uiThread == null)
				throw new Exception($"Attempt to check thread after dispatcher has been disposed.");

			return this.uiThread == Thread.CurrentThread;
		}
	}

	public long Now => this.clock.ElapsedMilliseconds;
	public bool CanQueryPendingInput => true;
	public bool HasPendingInput => false;

	public void Signal()
	{
		this.isSignal = true;
	}

	public void UpdateTimer(long? dueTimeInMs)
	{
		this.timerMs = dueTimeInMs;
		this.timer.Start();
	}

	public void RunLoop(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			if (this.isSignal)
			{
				try
				{
					this.Signaled?.Invoke();
				}
				catch (Exception ex)
				{
					Studio.Log.Error(ex, "Error in dispatcher signal");
				}

				this.isSignal = false;
			}

			if (this.timerMs != null && this.timer.ElapsedMilliseconds > this.timerMs)
			{
				try
				{
					this.Timer?.Invoke();
				}
				catch (Exception ex)
				{
					Studio.Log.Error(ex, "Error in dispatcher timer");
				}

				this.timerMs = null;
			}

			Studio.Tick.OnUiTick();

			Thread.Sleep(1000 / 60);
		}

		this.uiThread = null;
		this.timer.Stop();

		FieldInfo? uiDispatcher = typeof(Dispatcher).GetField("s_uiThread", BindingFlags.NonPublic | BindingFlags.Static);
		if (uiDispatcher == null)
		{
			Studio.Log.Error("Failed to find UI Thread dispatcher field");
		}
		else
		{
			uiDispatcher.SetValue(null, null);
		}

		Studio.Log.Information("Dispatcher terminated");
	}
}
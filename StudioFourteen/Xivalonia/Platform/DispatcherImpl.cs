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

namespace StudioFourteen.Xivalonia.Platform;

using System;
using System.Diagnostics;
using System.Threading;
using Avalonia.Threading;

[Logger]
public partial class DispatcherImpl : IControlledDispatcherImpl
{
	private static Thread? uiThread;
	private static bool isSignal = false;
	private static long? timerMs = null;

	private readonly Stopwatch clock = Stopwatch.StartNew();
	private readonly Stopwatch timer = new Stopwatch();

	public DispatcherImpl()
	{
		uiThread = Thread.CurrentThread;
	}

	public event Action? Signaled;
	public event Action? Timer;

	public bool CurrentThreadIsLoopThread => uiThread == Thread.CurrentThread;
	public long Now => this.clock.ElapsedMilliseconds;
	public bool CanQueryPendingInput => true;
	public bool HasPendingInput => false;

	public void Signal()
	{
		isSignal = true;
	}

	public void UpdateTimer(long? dueTimeInMs)
	{
		timerMs = dueTimeInMs;
		this.timer.Start();
	}

	public void RunLoop(CancellationToken token)
	{
		while (!token.IsCancellationRequested)
		{
			if (isSignal)
			{
				try
				{
					this.Signaled?.Invoke();
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Error in dispatcher signal");
				}

				isSignal = false;
			}

			if (timerMs != null && this.timer.ElapsedMilliseconds > timerMs)
			{
				try
				{
					this.Timer?.Invoke();
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Error in dispatcher timer");
				}

				timerMs = null;
			}

			Thread.Sleep(1000 / 60);
		}

		this.Log.Information("Dispatcher terminated");
	}
}
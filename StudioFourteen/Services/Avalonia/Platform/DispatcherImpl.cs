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

namespace StudioFourteen.Services.Avalonia.Platform;

using System;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using global::Avalonia.Threading;
using StudioFourteen.Services.Tick;

public partial class DispatcherImpl : IControlledDispatcherImpl
{
	private readonly AutoResetEvent wakeup = new(false);
	private readonly Lock @lock = new();
	private readonly Stopwatch clock = Stopwatch.StartNew();
	private readonly Thread loopThread = Thread.CurrentThread;

	private bool signaled;
	private TimeSpan? nextTimer;

	public event Action? Signaled;
	public event Action? Timer;

	public bool CurrentThreadIsLoopThread => this.loopThread == Thread.CurrentThread;
	public long Now => this.clock.ElapsedMilliseconds;

	public bool CanQueryPendingInput => true;
	public bool HasPendingInput => false;

	public void Signal()
	{
		lock (this.@lock)
		{
			this.signaled = true;
			this.wakeup.Set();
		}
	}

	public void UpdateTimer(long? dueTimeInMs)
	{
		lock (this.@lock)
		{
			this.nextTimer = dueTimeInMs == null
				? null
				: TimeSpan.FromMilliseconds(dueTimeInMs.Value);
			if (!this.CurrentThreadIsLoopThread)
				this.wakeup.Set();
		}
	}

	public void RunLoop(CancellationToken token)
	{
		CancellationTokenRegistration registration = default;
		if (token.CanBeCanceled)
			registration = token.Register(() => this.wakeup.Set());

		while (!token.IsCancellationRequested)
		{
			bool signaled;
			lock (this.@lock)
			{
				signaled = this.signaled;
				this.signaled = false;
			}

			if (signaled)
			{
				this.Signaled?.Invoke();
				continue;
			}

			bool fireTimer = false;
			lock (this.@lock)
			{
				if (this.nextTimer < this.clock.Elapsed)
				{
					fireTimer = true;
					this.nextTimer = null;
				}
			}

			if (fireTimer)
			{
				this.Timer?.Invoke();
				continue;
			}

			TimeSpan? nextTimer;
			lock (this.@lock)
			{
				nextTimer = this.nextTimer;
			}

			if (nextTimer != null)
			{
				var waitFor = nextTimer.Value - this.clock.Elapsed;
				if (waitFor.TotalMilliseconds < 1)
					continue;
				this.wakeup.WaitOne(waitFor);
			}
			else
			{
				this.wakeup.WaitOne();
			}
		}

		registration.Dispose();
	}
}
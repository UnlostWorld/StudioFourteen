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

namespace StudioFourteen.Services;

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using StudioFourteen.Interop;

using Task = System.Threading.Tasks.Task;

public partial class TickService : ServiceBase
{
	private const int TickDelay = 100;

	[ThreadStatic] private static TickService.Channels currentChannel;

	private readonly Dictionary<Channels, List<Action>> tickListeners = new();
	private readonly Dictionary<Channels, Queue<Action>> tickDispatchers = new();
	private bool shouldTick = true;

	public delegate void TickDelegate();

	public event TickDelegate? Tick;

	public enum Channels
	{
		GameTick,
		StudioTick,
	}

	public static SwitchToTickChannel GameTick() => new(TickService.Channels.GameTick);
	public static SwitchToTickChannel StudioTick() => new(TickService.Channels.StudioTick);

	public static void VerifyGameTickThread() => VerifyTickChannelThread(TickService.Channels.GameTick);
	public static void VerifyStudioTickThread() => VerifyTickChannelThread(TickService.Channels.StudioTick);

	public static void VerifyTickChannelThread(TickService.Channels channel)
	{
		if (currentChannel != channel)
		{
			throw new InvalidThreadException();
		}
	}

	public unsafe override void Attach()
	{
		base.Attach();
		Hooks.Tick.Enable(this.OnGameTick);
	}

	public override void Detach()
	{
		base.Detach();
		Hooks.Tick.Disable();
	}

	public override Task Initialize()
	{
		Thread panelMainThread = new Thread(this.TickThread);
		panelMainThread.Start();

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.shouldTick = false;

		foreach((Channels chanel, List<Action> callbacks) in this.tickListeners)
		{
			foreach(Action action in callbacks)
			{
				this.Log.Warning($"Tick listener: {action} not removed before shutdown.");
			}
		}

		return base.Shutdown();
	}

	public void Dispatch(Channels channel, Action callback)
	{
		if (!this.tickDispatchers.ContainsKey(channel))
			this.tickDispatchers.Add(channel, new());

		this.tickDispatchers[channel].Enqueue(callback);
	}

	public void Add(Channels channel, Action callback)
	{
		if (!this.tickListeners.ContainsKey(channel))
			this.tickListeners.Add(channel, new());

		this.tickListeners[channel].Add(callback);
	}

	public void Remove(Channels channel, Action callback)
	{
		if (!this.tickListeners.ContainsKey(channel))
			return;

		this.tickListeners[channel].Remove(callback);
	}

	private void PerformTick(Channels channel)
	{
		currentChannel = channel;

		this.tickListeners.TryGetValue(channel, out var callbacks);
		if (callbacks != null)
		{
			foreach(Action callback in this.tickListeners[channel].ToArray())
			{
				if (callback.Target == null)
				{
					this.tickListeners[channel].Remove(callback);
					break;
				}

				try
				{
					callback.Invoke();
				}
				catch(Exception ex)
				{
					this.Log.Error(ex, $"Error ticking {callback}. This callback will be disabled.");
					this.tickListeners[channel].Remove(callback);
					break;
				}
			}
		}

		this.tickDispatchers.TryGetValue(channel, out var dispatches);
		if (dispatches != null)
		{
			while(dispatches.Count > 0)
			{
				Action dispatch = dispatches.Dequeue();
				try
				{
					dispatch.Invoke();
				}
				catch(Exception ex)
				{
					this.Log.Error(ex, $"Error dispatching {dispatch}.");
					break;
				}
			}
		}
	}

	private unsafe bool OnGameTick(Framework* pFramework)
	{
		this.PerformTick(Channels.GameTick);
		return Hooks.Tick.Original(pFramework);
	}

	private void TickThread()
	{
		while(this.shouldTick)
		{
			Thread.Sleep(TickDelay);
			this.PerformTick(Channels.StudioTick);
		}
	}

	public struct SwitchToTickChannel(TickService.Channels channel)
		: INotifyCompletion
	{
		public bool IsCompleted => currentChannel == channel;

		public SwitchToTickChannel GetAwaiter() => this;
		public readonly void GetResult()
		{
		}

		public readonly void OnCompleted(Action continuation)
		{
			if (ServiceManager.ShutdownRequested)
				return;

			ServiceManager.Instance.Tick.Dispatch(channel, continuation);
		}
	}
}

public class InvalidThreadException : Exception
{
	public InvalidThreadException()
		: base("Invalid Thread")
	{
	}
}
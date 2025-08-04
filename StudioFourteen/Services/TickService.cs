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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using StudioFourteen.Interop;
using Windows.Win32;
using Task = System.Threading.Tasks.Task;

public partial class TickService : ServiceBase
{
	private const int TickDelay = 100;

	[ThreadStatic] private static TickService.Channels currentChannel = Channels.None;

	private readonly Dictionary<Channels, List<Action?>> tickListeners = new();
	private readonly Dictionary<Channels, Queue<Action?>> tickDispatchers = new();
	private bool shouldTick = true;

	public delegate void TickDelegate();

	public event TickDelegate? Tick;

	public enum Channels
	{
		None,

		EarlyGameTick,
		GameTick,
		LateGameTick,
		StudioTick,
		ImGuiDraw,
	}

	public static SwitchToTickChannel EarlyGameTick() => new(TickService.Channels.EarlyGameTick);
	public static SwitchToTickChannel GameTick() => new(TickService.Channels.GameTick);
	public static SwitchToTickChannel LateGameTick() => new(TickService.Channels.LateGameTick);
	public static SwitchToTickChannel NextGameTick() => new(TickService.Channels.GameTick);
	public static SwitchToTickChannel StudioTick() => new(TickService.Channels.StudioTick);
	public static SwitchToTickChannel NextStudioTick() => new(TickService.Channels.StudioTick);

	public static void VerifyGameTickThread() => VerifyTickChannelThread(TickService.Channels.EarlyGameTick, TickService.Channels.GameTick, TickService.Channels.LateGameTick);
	public static void VerifyStudioTickThread() => VerifyTickChannelThread(TickService.Channels.StudioTick);

	public static void VerifyTickChannelThread(params TickService.Channels[] channels)
	{
		if (!channels.Contains(currentChannel))
		{
			throw new InvalidThreadException();
		}
	}

	public override void Dispose()
	{
		Hooks.Tick.Disable();
		base.Dispose();
	}

	public override Task Initialize()
	{
		unsafe
		{
			Hooks.Tick.Enable(this.OnGameTick);
		}

		Thread panelMainThread = new Thread(this.TickThread);
		panelMainThread.Start();

		if (Plugin.DalamudServices.PluginInterface != null)
			Plugin.DalamudServices.PluginInterface.UiBuilder.Draw += this.OnImGuiDraw;

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.shouldTick = false;

		Hooks.Tick.Disable();

		if (Plugin.DalamudServices.PluginInterface != null)
			Plugin.DalamudServices.PluginInterface.UiBuilder.Draw -= this.OnImGuiDraw;

		foreach((Channels chanel, List<Action?> callbacks) in this.tickListeners)
		{
			foreach(Action? action in callbacks)
			{
				this.Log.Warning($"Tick listener: {action?.Method} on {action?.Target} not removed before shutdown.");
			}
		}

		return base.Shutdown();
	}

	public void Dispatch(Channels channel, Action callback, bool canImmediate = true)
	{
		if (currentChannel == channel && canImmediate)
		{
			callback.Invoke();
		}
		else
		{
			lock (this.tickDispatchers)
			{
				if (!this.tickDispatchers.ContainsKey(channel))
					this.tickDispatchers.Add(channel, new());

				this.tickDispatchers[channel].Enqueue(callback);
			}
		}
	}

	public void Add(Channels channel, Action callback)
	{
		lock (this.tickListeners)
		{
			if (!this.tickListeners.ContainsKey(channel))
				this.tickListeners.Add(channel, new());

			this.tickListeners[channel].Add(callback);
		}
	}

	public void Remove(Channels channel, Action callback)
	{
		lock (this.tickListeners)
		{
			if (!this.tickListeners.ContainsKey(channel))
				return;

			this.tickListeners[channel].Remove(callback);
		}
	}

	private void PerformTick(Channels channel)
	{
		currentChannel = channel;

		Dictionary<Channels, List<Action?>> tickListeners;
		lock(this.tickListeners)
		{
			tickListeners = new(this.tickListeners);
		}

		this.tickListeners.TryGetValue(channel, out var callbacks);
		if (callbacks != null)
		{
			foreach(Action? callback in this.tickListeners[channel].ToArray())
			{
				if (callback?.Target == null)
				{
					this.tickListeners[channel].Remove(callback);
					break;
				}

				try
				{
					callback?.Invoke();
				}
				catch(Exception ex)
				{
					this.Log.Error(ex, $"Error ticking {callback?.Method} on {callback?.Target}. This callback will be disabled.");
					this.tickListeners[channel].Remove(callback);
					break;
				}
			}
		}

		Dictionary<Channels, Queue<Action?>> tickDispatchers;
		lock (this.tickDispatchers)
		{
			tickDispatchers = new(this.tickDispatchers);
		}

		this.tickDispatchers.TryGetValue(channel, out var dispatches);
		if (dispatches != null)
		{
			while(dispatches.Count > 0)
			{
				Action? dispatch = dispatches.Dequeue();
				try
				{
					dispatch?.Invoke();
				}
				catch(Exception ex)
				{
					this.Log.Error(ex, $"Error dispatching {dispatch?.Method} on {dispatch?.Target}.");
					break;
				}
			}
		}
	}

	private unsafe bool OnGameTick(Framework* pFramework)
	{
		this.PerformTick(Channels.EarlyGameTick);
		this.PerformTick(Channels.GameTick);
		this.PerformTick(Channels.LateGameTick);
		return Hooks.Tick.Original(pFramework);
	}

	private void TickThread()
	{
		while(this.shouldTick && !ServiceManager.ShutdownRequested)
		{
			Thread.Sleep(TickDelay);
			this.PerformTick(Channels.StudioTick);
		}
	}

	private void OnImGuiDraw()
	{
		this.PerformTick(Channels.ImGuiDraw);
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
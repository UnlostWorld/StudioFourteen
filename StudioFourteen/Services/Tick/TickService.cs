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

namespace StudioFourteen.Services.Tick;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using StudioFourteen.Services.Interop;
using Task = System.Threading.Tasks.Task;

public enum TickChannels
{
	None,

	EarlyGame,
	Game,
	LateGame,
	Studio,
	ImGuiDraw,
	Ui,
}

public partial class TickService : IService
{
	public static float DeltaTime = 0.0f;

	private const int TickDelay = 100;

	[ThreadStatic] private static TickChannels currentChannel = TickChannels.None;

	private readonly Dictionary<TickChannels, List<Action?>> tickListeners = new();
	private readonly Dictionary<TickChannels, Queue<Action?>> tickDispatchers = new();
	private bool shouldTick = true;

	public TickService()
	{
		unsafe
		{
			Hooks.Tick.Enable(this.OnGameTick);
		}

		Thread panelMainThread = new Thread(this.TickThread);
		panelMainThread.Start();

		Studio.PluginInterface.UiBuilder.Draw += this.OnImGuiDraw;
	}

	public delegate void TickDelegate();

	public event TickDelegate? Tick;

	public static SwitchToTickChannel EarlyGameTick() => new(TickChannels.EarlyGame);
	public static SwitchToTickChannel GameTick() => new(TickChannels.Game);
	public static SwitchToTickChannel LateGameTick() => new(TickChannels.LateGame);
	public static SwitchToTickChannel NextGameTick() => new(TickChannels.Game);
	public static SwitchToTickChannel StudioTick() => new(TickChannels.Studio);
	public static SwitchToTickChannel NextStudioTick() => new(TickChannels.Studio);
	public static SwitchToTickChannel UiTick() => new(TickChannels.Ui);

	public static void VerifyGameTickThread() => VerifyTickChannelThread(TickChannels.EarlyGame, TickChannels.Game, TickChannels.LateGame);
	public static void VerifyStudioTickThread() => VerifyTickChannelThread(TickChannels.Studio);
	public static void VerifyUiTickThread() => VerifyTickChannelThread(TickChannels.Ui);

	public static void VerifyTickChannelThread(params TickChannels[] channels)
	{
		if (!channels.Contains(currentChannel))
		{
			throw new InvalidThreadException();
		}
	}

	public void Dispose()
	{
		this.shouldTick = false;

		Hooks.Tick.Disable();
		Studio.PluginInterface.UiBuilder.Draw -= this.OnImGuiDraw;
	}

	public void Dispatch(TickChannels channel, Action callback, bool canImmediate = true)
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

	public void Add(TickChannels channel, Action callback)
	{
		lock (this.tickListeners)
		{
			if (!this.tickListeners.ContainsKey(channel))
				this.tickListeners.Add(channel, new());

			this.tickListeners[channel].Add(callback);
		}
	}

	public void Remove(TickChannels channel, Action callback)
	{
		lock (this.tickListeners)
		{
			if (!this.tickListeners.ContainsKey(channel))
				return;

			this.tickListeners[channel].Remove(callback);
		}
	}

	internal void OnUiTick()
	{
		this.PerformTick(TickChannels.Ui);
	}

	private void PerformTick(TickChannels channel)
	{
		currentChannel = channel;

		Dictionary<TickChannels, List<Action?>> tickListeners;
		lock (this.tickListeners)
		{
			tickListeners = new(this.tickListeners);
		}

		this.tickListeners.TryGetValue(channel, out var callbacks);
		if (callbacks != null)
		{
			foreach (Action? callback in this.tickListeners[channel].ToArray())
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
				catch (Exception ex)
				{
					Studio.Log.Error(ex, $"Error ticking {callback?.Method} on {callback?.Target}. This callback will be disabled.");
					this.tickListeners[channel].Remove(callback);
					break;
				}
			}
		}

		Dictionary<TickChannels, Queue<Action?>> tickDispatchers;
		lock (this.tickDispatchers)
		{
			tickDispatchers = new(this.tickDispatchers);
		}

		this.tickDispatchers.TryGetValue(channel, out var dispatches);
		if (dispatches != null)
		{
			while (dispatches.Count > 0)
			{
				Action? dispatch = dispatches.Dequeue();
				try
				{
					dispatch?.Invoke();
				}
				catch (Exception ex)
				{
					Studio.Log.Error(ex, $"Error dispatching {dispatch?.Method} on {dispatch?.Target}.");
					break;
				}
			}
		}
	}

	private unsafe bool OnGameTick(Framework* pFramework)
	{
		Thread.CurrentThread.Name = "Game Tick";

		DeltaTime = pFramework->FrameDeltaTime;

		this.PerformTick(TickChannels.EarlyGame);
		this.PerformTick(TickChannels.Game);
		this.PerformTick(TickChannels.LateGame);
		return Hooks.Tick.Original(pFramework);
	}

	private void TickThread()
	{
		Thread.CurrentThread.Name = "Studio Tick";

		while (this.shouldTick && !Studio.IsDisposed)
		{
			Thread.Sleep(TickDelay);
			DeltaTime = TickDelay / 1000.0f;
			this.PerformTick(TickChannels.Studio);
		}
	}

	private void OnImGuiDraw()
	{
		this.PerformTick(TickChannels.ImGuiDraw);
	}

	public struct SwitchToTickChannel(TickChannels channel)
		: INotifyCompletion
	{
		public bool IsCompleted => currentChannel == channel;

		public SwitchToTickChannel GetAwaiter() => this;
		public readonly void GetResult()
		{
		}

		public readonly void OnCompleted(Action continuation)
		{
			if (Studio.IsDisposed)
				return;

			Studio.Tick.Dispatch(channel, continuation);
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
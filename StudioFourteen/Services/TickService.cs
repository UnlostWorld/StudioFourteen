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
using System.Threading;
using System.Threading.Tasks;
using StudioFourteen.Interop;

public partial class TickService : ServiceBase
{
	private const int TickDelay = 30 / 1000;

	private readonly Dictionary<Channels, List<Action>> tickListeners = new();
	private bool shouldTick = true;

	public delegate void TickDelegate();

	public event TickDelegate? Tick;

	public enum Channels
	{
		BeforeGameTick,
		AfterGameTick,

		GameTick = BeforeGameTick,

		StudioTick,
	}

	public override void Attach()
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
		panelMainThread.Start(this);

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
		this.tickListeners.TryGetValue(channel, out var callbacks);

		if (callbacks == null)
			return;

		foreach(Action callback in this.tickListeners[channel])
		{
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

	private bool OnGameTick()
	{
		this.PerformTick(Channels.BeforeGameTick);
		bool gameResult = Hooks.Tick.Original();
		this.PerformTick(Channels.AfterGameTick);
		return gameResult;
	}

	private void TickThread()
	{
		while(this.shouldTick)
		{
			Thread.Sleep(TickDelay);
			this.PerformTick(Channels.StudioTick);
		}
	}
}
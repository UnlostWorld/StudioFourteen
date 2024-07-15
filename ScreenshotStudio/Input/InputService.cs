// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Input/InputService.cs

namespace ScreenshotStudio.Input;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class InputService : ServiceBase
{
	private readonly HashSet<KeyBindEvents> eventsDown = new();
	private readonly Dictionary<KeyBindEvents, List<Action>> listeners = new();

	public bool EnableKeyBinds => true;

	public Dictionary<KeyBindEvents, KeyBind> Bindings { get; set; } = new()
	{
		// Default bindings
		{ KeyBindEvents.Interface_InvokeQuickSearch, new(VirtualKey.Q, false, false, true) },
	};

	public static IEnumerable<VirtualKey> GetValidKeys()
	{
		if (DalamudServices.KeyState == null)
			return new List<VirtualKey>();

		return DalamudServices.KeyState.GetValidVirtualKeys();
	}

	public static bool IsKeyBindDown(KeyBindEvents evt)
	{
		if (!ServiceManager.Instance.Input.EnableKeyBinds)
			return false;

		return ServiceManager.Instance.Input.eventsDown.Contains(evt);
	}

	public override Task Initialize()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		return base.Initialize();
	}

	public override Task Shutdown()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		return base.Shutdown();
	}

	public bool HasListener(KeyBindEvents evt)
	{
		if (!this.listeners.ContainsKey(evt))
			return false;

		return this.listeners[evt].Count > 0;
	}

	public void AddListener(KeyBindEvents evt, Action callback)
	{
		if (!this.listeners.ContainsKey(evt))
			this.listeners.Add(evt, new());

		this.listeners[evt].Add(callback);
	}

	public void RemoveListener(KeyBindEvents evt, Action callback)
	{
		if (!this.listeners.ContainsKey(evt))
			return;

		this.listeners[evt].Remove(callback);
	}

	public KeyBind? GetKeyBind(KeyBindEvents evt)
	{
		KeyBind? bind = null;
		this.Bindings.TryGetValue(evt, out bind);
		return bind;
	}

	private void OnFrameworkUpdate(IFramework framework)
	{
		if (!this.Services.Studio.IsOpenAndInGPose)
			return;

		if (!this.EnableKeyBinds)
			return;

		foreach (var evt in Enum.GetValues<KeyBindEvents>())
		{
			this.CheckEvent(evt);
		}
	}

	private void CheckEvent(KeyBindEvents evt)
	{
		KeyBind? bind;
		if (!this.Bindings.TryGetValue(evt, out bind) || bind == null)
			return;

		bool isDown = this.IsDown(bind);
		bool wasDown = this.eventsDown.Contains(evt);

		if (!isDown && wasDown)
		{
			this.eventsDown.Remove(evt);
		}
		else if (isDown && !wasDown)
		{
			this.eventsDown.Add(evt);

			try
			{
				// just released, invoke listeners
				foreach (Action callback in this.listeners[evt])
				{
					callback?.Invoke();
				}
			}
			catch (Exception)
			{
			}
		}
	}

	private bool IsDown(KeyBind bind)
	{
		if (DalamudServices.KeyState == null)
			return false;

		if (bind.Key == VirtualKey.NO_KEY)
			return false;

		bool down = DalamudServices.KeyState[bind.Key];

		if (bind.Key != VirtualKey.CONTROL)
			down &= DalamudServices.KeyState[VirtualKey.CONTROL] == bind.Control;

		if (bind.Key != VirtualKey.MENU)
			down &= DalamudServices.KeyState[VirtualKey.MENU] == bind.Alt;

		if (bind.Key != VirtualKey.SHIFT)
			down &= DalamudServices.KeyState[VirtualKey.SHIFT] == bind.Shift;

		return down;
	}
}

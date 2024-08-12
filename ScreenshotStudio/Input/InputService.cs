// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Input/InputService.cs

namespace ScreenshotStudio.Input;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Input;

public class InputService : ServiceBase
{
	private readonly HashSet<KeyBindEvents> eventsDown = new();
	private readonly Dictionary<KeyBindEvents, List<Action>> listeners = new();
	private readonly HashSet<VirtualKey> windowsKeys = new();

	public bool EnableKeyBinds => true;

	public bool XivWindowIsActive { get; private set; }
	public bool IsTextInputActive { get; private set; }

	public Dictionary<KeyBindEvents, KeyBind> Bindings { get; set; } = new()
	{
		// Default bindings
		{ KeyBindEvents.InvokeQuickSearch, new(VirtualKey.Q, false, false, true) },
		{ KeyBindEvents.Save, new(VirtualKey.S, true, false, false) },
		{ KeyBindEvents.SaveAs, new(VirtualKey.S, true, false, true) },
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

	public void SetKeyDown(Key key, bool isDown)
	{
		VirtualKey virtualKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(key);

		// FFXIV doesn't support L/R modifiers, so neither do we. =(
		if (virtualKey == VirtualKey.LSHIFT || virtualKey == VirtualKey.RSHIFT)
			virtualKey = VirtualKey.SHIFT;

		if (virtualKey == VirtualKey.LCONTROL || virtualKey == VirtualKey.RCONTROL)
			virtualKey = VirtualKey.CONTROL;

		if (virtualKey == VirtualKey.LMENU || virtualKey == VirtualKey.RMENU)
			virtualKey = VirtualKey.MENU;

		if (isDown)
		{
			this.windowsKeys.Add(virtualKey);
		}
		else
		{
			this.windowsKeys.Remove(virtualKey);
		}
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (!this.Services.Studio.IsOpen)
			return;

		this.XivWindowIsActive = XivWindow.IsActive();
		this.IsTextInputActive = RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

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

		this.listeners.TryGetValue(evt, out List<Action>? listeners);
		if (listeners == null || listeners.Count == 0)
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
				// just pressed, invoke listeners
				foreach (Action callback in listeners)
				{
					if (callback == null)
						continue;

					callback.Invoke();
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error in event {evt} listener");
			}
		}

		if (isDown)
		{
			this.ResetBindKeys(bind);
		}
	}

	private bool IsDown(KeyBind bind)
	{
		if (bind.Key == VirtualKey.NO_KEY)
			return false;

		bool down = this.IsDown(bind.Key);

		if (bind.Key != VirtualKey.CONTROL)
			down &= this.IsDown(VirtualKey.CONTROL) == bind.Control;

		if (bind.Key != VirtualKey.MENU)
			down &= this.IsDown(VirtualKey.MENU) == bind.Alt;

		if (bind.Key != VirtualKey.SHIFT)
			down &= this.IsDown(VirtualKey.SHIFT) == bind.Shift;

		return down;
	}

	private bool IsDown(VirtualKey key)
	{
		if (key == VirtualKey.NO_KEY)
			return false;

		if (!this.XivWindowIsActive)
		{
			return this.windowsKeys.Contains(key);
		}
		else
		{
			if (DalamudServices.KeyState == null)
				return false;

			return DalamudServices.KeyState[key];
		}
	}

	private void ResetBindKeys(KeyBind bind)
	{
		if (DalamudServices.KeyState == null)
			return;

		DalamudServices.KeyState[bind.Key] = false;
	}
}

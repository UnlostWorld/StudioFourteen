namespace StudioFourteen.Input;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Serilog;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

public class InputService : ServiceBase
{
	private readonly Dictionary<KeyBindEvents, List<KeyBindListener>> listeners = new();
	private readonly Dictionary<MouseButton, States> mouseButtons = new();
	private readonly Dictionary<VirtualKey, States> keyboardKeys = new();
	private readonly KeyState xivKeyState = new();

	public delegate void MouseDragDelegate(Vector2 delta, MouseButton button);
	public delegate void MouseWheelDelegate(float delta);

	public event MouseDragDelegate? MouseDrag;
	public event MouseWheelDelegate? MouseWheel;

	public enum States
	{
		Up,
		Down,
		Pressed,
		Released,
	}

	public bool IsXivTextInputActive { get; private set; }
	public bool IsStudioTextInputActive
	{
		get
		{
			if (Keyboard.FocusedElement is TextBoxBase tb)
			{
				return tb.IsFocused && (tb.IsKeyboardFocused || tb.IsKeyboardFocusWithin);
			}

			return false;
		}
	}

	public Vector2 MousePosition { get; private set; }

	public Dictionary<KeyBindEvents, KeyBind> DefaultKeys { get; init; } = new()
	{
		{ KeyBindEvents.InvokeQuickSearch, new(VirtualKey.Q, ModifierKeys.Shift) },
		{ KeyBindEvents.Save, new(VirtualKey.S, ModifierKeys.Control) },
		{ KeyBindEvents.SaveAs, new(VirtualKey.S, ModifierKeys.Control | ModifierKeys.Shift) },

		// Free Camera
		{ KeyBindEvents.FreeCamera_MoveForwards, new(VirtualKey.W) },
		{ KeyBindEvents.FreeCamera_MoveBack, new(VirtualKey.S) },
		{ KeyBindEvents.FreeCamera_MoveLeft, new(VirtualKey.A) },
		{ KeyBindEvents.FreeCamera_MoveRight, new(VirtualKey.D) },
		{ KeyBindEvents.FreeCamera_MoveUp, new(VirtualKey.Q) },
		{ KeyBindEvents.FreeCamera_MoveDown, new(VirtualKey.E) },
		{ KeyBindEvents.FreeCamera_YawLeft, new(VirtualKey.A, ModifierKeys.Shift) },
		{ KeyBindEvents.FreeCamera_YawRight, new(VirtualKey.D, ModifierKeys.Shift) },
		{ KeyBindEvents.FreeCamera_PitchUp, new(VirtualKey.W, ModifierKeys.Shift) },
		{ KeyBindEvents.FreeCamera_PitchDown, new(VirtualKey.S, ModifierKeys.Shift) },
		{ KeyBindEvents.FreeCamera_RollLeft, new(VirtualKey.Q, ModifierKeys.Shift) },
		{ KeyBindEvents.FreeCamera_RollRight, new(VirtualKey.E, ModifierKeys.Shift) },

		// Orbit Camera
		{ KeyBindEvents.OrbitCamera_PanUp, new(VirtualKey.W) },
		{ KeyBindEvents.OrbitCamera_PanDown, new(VirtualKey.S) },
		{ KeyBindEvents.OrbitCamera_PanLeft, new(VirtualKey.A) },
		{ KeyBindEvents.OrbitCamera_PanRight, new(VirtualKey.D) },
		{ KeyBindEvents.OrbitCamera_RollLeft, new(VirtualKey.Q) },
		{ KeyBindEvents.OrbitCamera_RollRight, new(VirtualKey.E) },
		{ KeyBindEvents.OrbitCamera_MoveUp, new(VirtualKey.W, ModifierKeys.Shift) },
		{ KeyBindEvents.OrbitCamera_MoveDown, new(VirtualKey.S, ModifierKeys.Shift) },
		{ KeyBindEvents.OrbitCamera_MoveLeft, new(VirtualKey.A, ModifierKeys.Shift) },
		{ KeyBindEvents.OrbitCamera_MoveRight, new(VirtualKey.D, ModifierKeys.Shift) },
	};

	public override void Attach()
	{
		this.xivKeyState.Attach();
		base.Attach();
	}

	public override void Detach()
	{
		this.xivKeyState.Detach();
		base.Detach();
	}

	public IEnumerable<VirtualKey> GetValidKeys()
	{
		return this.xivKeyState.GetValidVirtualKeys();
	}

	public bool HasListener(KeyBindEvents evt)
	{
		if (!this.listeners.ContainsKey(evt))
			return false;

		return this.listeners[evt].Count > 0;
	}

	public void AddListener(KeyBindEvents evt, KeyBindListener listener)
	{
		if (!this.listeners.ContainsKey(evt))
			this.listeners.Add(evt, new());

		this.listeners[evt].Add(listener);
	}

	public void RemoveListener(KeyBindEvents evt, KeyBindListener listener)
	{
		if (!this.listeners.ContainsKey(evt))
			return;

		this.listeners[evt].Remove(listener);
	}

	public KeyBind? GetKeyBind(KeyBindEvents evt)
	{
		KeyBind? bind = null;
		if (!this.Settings.CustomKeyBinds.TryGetValue(evt, out bind))
		{
			this.DefaultKeys.TryGetValue(evt, out bind);
		}

		return bind;
	}

	public bool IsMouseDown(MouseButton button)
	{
		if (this.mouseButtons.TryGetValue(button, out States value))
			return value == States.Down;

		return false;
	}

	public void HandleMouse(MouseButtonEventArgs e, bool down)
	{
		this.mouseButtons[e.ChangedButton] = down ? States.Pressed : States.Released;

		if (!down)
		{
			CursorUtility.SetCursorVisible(true);
		}
	}

	public void HandleMouseMove(Vector2 newPos)
	{
		Vector2 delta = newPos - this.MousePosition;

		bool holdPosition = false;
		foreach ((MouseButton button, States state) in this.mouseButtons)
		{
			if (state == States.Down)
			{
				holdPosition = true;
				this.MouseDrag?.Invoke(delta, button);
				this.Services.Windows.SetCursorPosition(new(this.MousePosition.X, this.MousePosition.Y));
			}
		}

		if (holdPosition)
		{
			CursorUtility.SetCursorVisible(false);
		}
		else
		{
			this.MousePosition = newPos;
		}
	}

	public void HandleMouseLeave()
	{
		foreach((MouseButton button, States state) in this.mouseButtons)
		{
			this.mouseButtons[button] = States.Up;
		}

		CursorUtility.SetCursorVisible(true);
	}

	public void HandleMouseWheel(float delta)
	{
		this.MouseWheel?.Invoke(delta);
	}

	public void HandleKey(Key key, bool down)
	{
		VirtualKey vKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(key);
		if (vKey == VirtualKey.NO_KEY)
			return;

		if (!this.keyboardKeys.ContainsKey(vKey))
			this.keyboardKeys[vKey] = States.Up;

		if (down)
		{
			if (this.keyboardKeys[vKey] == States.Up)
			{
				this.keyboardKeys[vKey] = States.Pressed;
			}
		}
		else
		{
			if (this.keyboardKeys[vKey] == States.Down)
			{
				this.keyboardKeys[vKey] = States.Released;
			}
		}

		if (down)
		{
			if (Keyboard.FocusedElement is TextBoxBase tb)
			{
				if (tb.IsFocused && (tb.IsKeyboardFocused || tb.IsKeyboardFocusWithin))
				{
					if (key == Key.Escape)
					{
						tb.SetFocusToWindow();
					}

					return;
				}
			}
		}
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (!this.Services.Studio.IsOpen)
			return;

		this.IsXivTextInputActive = RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

		HashSet<VirtualKey> usedKeys = new();
		foreach (var evt in Enum.GetValues<KeyBindEvents>())
		{
			this.CheckEvent(evt, ref usedKeys);
		}

		if (this.Services.Panels.ActivePanel == null && this.Services.Windows.IsXivWindowActive())
		{
			// Read xiv -> studio
			foreach(VirtualKey key in this.xivKeyState.GetValidVirtualKeys())
			{
				KeyState.KeyValue state = this.xivKeyState[key];

				this.keyboardKeys[key] = state switch
				{
					KeyState.KeyValue.Up => States.Up,
					KeyState.KeyValue.Down => States.Down,
					KeyState.KeyValue.Pressed => States.Pressed,
					KeyState.KeyValue.Released => States.Released,
					KeyState.KeyValue.Unk => States.Up,
					_ => throw new InvalidOperationException(),
				};
			}
		}
		else
		{
			// Write studio -> xiv
			foreach ((VirtualKey key, States state) in this.keyboardKeys)
			{
				if (usedKeys.Contains(key))
					continue;

				if (!this.xivKeyState.IsVirtualKeyValid(key))
					continue;

				// Only set the pressed state into xiv as its input system will handle the rest.
				// We only support forwarding keys as single presses, no holds, since xiv will constantly
				// set the values back in its own update loop.
				if (state == States.Pressed)
				{
					this.Services.Windows.ActivateXivWindow();
					this.xivKeyState[key] = KeyState.KeyValue.Pressed;
				}

				/*this.xivKeyState[key] = state switch
				{
					States.Up => KeyState.KeyValue.Up,
					States.Down => KeyState.KeyValue.Down,
					States.Pressed => KeyState.KeyValue.Pressed,
					States.Released => KeyState.KeyValue.Released,
					_ => throw new InvalidOperationException(),
				};*/
			}

			foreach ((VirtualKey key, States state) in this.keyboardKeys)
			{
				if (state == States.Pressed)
					this.keyboardKeys[key] = States.Down;

				if (state == States.Released)
					this.keyboardKeys[key] = States.Up;
			}

			foreach ((MouseButton button, States state) in this.mouseButtons)
			{
				if (state == States.Pressed)
					this.mouseButtons[button] = States.Down;

				if (state == States.Released)
					this.mouseButtons[button] = States.Up;
			}
		}
	}

	private bool CheckEvent(KeyBindEvents evt, ref HashSet<VirtualKey> usedKeys)
	{
		KeyBind? bind = this.GetKeyBind(evt);
		if (bind == null)
			return false;

		this.listeners.TryGetValue(evt, out List<KeyBindListener>? listeners);
		if (listeners == null || listeners.Count == 0)
			return false;

		States state = this.GetState(bind);
		foreach (KeyBindListener listener in listeners)
		{
			listener.SetState(state);
		}

		usedKeys.Add(bind.Key);

		return true;
	}

	private States GetState(KeyBind bind)
	{
		if (bind.Key == VirtualKey.NO_KEY)
			return States.Up;

		States state = this.GetState(bind.Key);
		bool hasModifiers = true;

		if (bind.Key != VirtualKey.CONTROL)
			hasModifiers &= (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)) == bind.Control;

		if (bind.Key != VirtualKey.MENU)
			hasModifiers &= (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt)) == bind.Alt;

		if (bind.Key != VirtualKey.SHIFT)
			hasModifiers &= (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) == bind.Shift;

		if (hasModifiers)
			return state;

		return States.Up;
	}

	private States GetState(VirtualKey key)
	{
		if (key == VirtualKey.NO_KEY)
			return States.Up;

		if (this.keyboardKeys.TryGetValue(key, out States state))
			return state;

		return States.Up;
	}
}

public class KeyBindListener
{
	public readonly ILogger Log = Logging.ForContext<KeyBindListener>();

	private readonly KeyBindEvents keyBindEvent;
	private InputService.States currentState = InputService.States.Up;
	private InputService.States cacheState = InputService.States.Up;

	public KeyBindListener(KeyBindEvents evt)
	{
		this.keyBindEvent = evt;
	}

	public Action? Pressed { get; set; }
	public Action? Down { get; set; }
	public Action? Released { get; set; }

	public void Enable()
	{
		ServiceManager.Instance.Input.AddListener(this.keyBindEvent, this);
	}

	public void Disable()
	{
		ServiceManager.Instance.Input.RemoveListener(this.keyBindEvent, this);
	}

	public void SetState(InputService.States state)
	{
		this.currentState = state;

		if (state == InputService.States.Pressed)
		{
			this.cacheState = InputService.States.Pressed;
		}
		else if (state == InputService.States.Released)
		{
			this.cacheState = InputService.States.Released;
		}

		try
		{
			if (state == InputService.States.Pressed)
			{
				this.Pressed?.Invoke();
			}
			else if (state == InputService.States.Down)
			{
				this.Down?.Invoke();
			}
			else if (state == InputService.States.Released)
			{
				this.Released?.Invoke();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error invoking key bind callback for event {this.keyBindEvent}");
		}
	}

	public InputService.States GetState()
	{
		InputService.States state = this.cacheState;

		if (state == InputService.States.Pressed)
		{
			state = InputService.States.Down;
		}
		else if (state == InputService.States.Released)
		{
			state = InputService.States.Up;
		}

		return state;
	}

	public InputService.States GetCurrentState()
	{
		return this.currentState;
	}

	public bool IsDown()
	{
		return this.currentState == InputService.States.Down;
	}
}
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

	public delegate void MouseDragDelegate(Vector2 delta, MouseButton button);
	public delegate void MouseButtonDelegate(MouseButton button, States state, Vector2 position);
	public delegate void MouseWheelDelegate(float delta);

	public event MouseDragDelegate? MouseDrag;
	public event MouseWheelDelegate? MouseWheel;
	public event MouseButtonDelegate? MouseButton;

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
	public bool IsMouseDragging { get; private set; }

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

	public IEnumerable<VirtualKey> GetValidKeys()
	{
		if (DalamudServices.KeyState == null)
			return new List<VirtualKey>();

		return DalamudServices.KeyState.GetValidVirtualKeys();
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

		if (down || !this.IsMouseDragging)
			this.MouseButton?.Invoke(e.ChangedButton, this.mouseButtons[e.ChangedButton], this.MousePosition);

		if (this.IsMouseDragging)
		{
			this.IsMouseDragging = false;
		}

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
				this.Services.Windows.SetCursorPosition(new((int)this.MousePosition.X, (int)this.MousePosition.Y));
				this.IsMouseDragging = true;
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

		if (vKey == VirtualKey.LSHIFT || vKey == VirtualKey.RSHIFT)
			vKey = VirtualKey.SHIFT;

		if (vKey == VirtualKey.LMENU || vKey == VirtualKey.RMENU)
			vKey = VirtualKey.MENU;

		if (vKey == VirtualKey.LCONTROL || vKey == VirtualKey.RCONTROL)
			vKey = VirtualKey.CONTROL;

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

		if (DalamudServices.KeyState == null)
			return;

		if (!this.Services.Studio.IsOpen)
			return;

		bool wasActive = this.IsXivTextInputActive;
		this.IsXivTextInputActive = RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

		// If Text Input just activated, and we have focus, set focus to xiv.
		if (!wasActive && this.IsXivTextInputActive && this.Services.Panels.ActivePanel != null)
		{
			this.Services.Windows.ActivateXivWindow();
		}

		// If text input is still active, but we are taking focus, send the escape key to clear
		// the text input focus from xiv.
		if (wasActive && this.IsXivTextInputActive && this.Services.Panels.ActivePanel != null)
		{
			this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, true);
			this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, false);
		}

		if (this.IsXivTextInputActive)
			return;

		if (this.Services.Panels.ActivePanel == null && this.Services.Windows.IsXivWindowActive())
		{
			// Read xiv -> studio
			foreach(VirtualKey key in DalamudServices.KeyState.GetValidVirtualKeys())
			{
				bool isDown = DalamudServices.KeyState[key];
				this.keyboardKeys.TryAdd(key, States.Up);

				if (isDown)
				{
					switch (this.keyboardKeys[key])
					{
						case States.Released:
						case States.Up:
						{
							this.keyboardKeys[key] = States.Pressed;
							break;
						}

						case States.Down:
						case States.Pressed:
						{
							this.keyboardKeys[key] = States.Down;
							break;
						}
					}
				}
				else
				{
					switch (this.keyboardKeys[key])
					{
						case States.Down:
						case States.Pressed:
						{
							this.keyboardKeys[key] = States.Released;
							break;
						}

						case States.Released:
						case States.Up:
						{
							this.keyboardKeys[key] = States.Up;
							break;
						}
					}
				}
			}

			HashSet<VirtualKey> usedKeys = new();
			foreach (var evt in Enum.GetValues<KeyBindEvents>())
			{
				this.CheckEvent(evt, ref usedKeys);
			}

			foreach(VirtualKey vKey in usedKeys)
			{
				if (DalamudServices.KeyState[vKey])
				{
					this.Services.Windows.ActivateStudioWindow();
					DalamudServices.KeyState[vKey] = false;
				}
			}
		}
		else
		{
			HashSet<VirtualKey> usedKeys = new();
			foreach (var evt in Enum.GetValues<KeyBindEvents>())
			{
				this.CheckEvent(evt, ref usedKeys);
			}

			// Write studio -> xiv
			foreach ((VirtualKey key, States state) in this.keyboardKeys)
			{
				if (usedKeys.Contains(key))
					continue;

				if (!DalamudServices.KeyState.IsVirtualKeyValid(key))
					continue;

				if (state == States.Pressed)
				{
					this.Services.Windows.SendKeyToXiv(key, true);
				}
				else if (state == States.Released)
				{
					this.Services.Windows.SendKeyToXiv(key, false);
				}
			}

			foreach ((VirtualKey key, States state) in this.keyboardKeys)
			{
				if (state == States.Pressed)
					this.keyboardKeys[key] = States.Down;

				if (state == States.Released)
					this.keyboardKeys[key] = States.Up;
			}
		}

		foreach ((MouseButton button, States state) in this.mouseButtons)
		{
			if (state == States.Pressed)
				this.mouseButtons[button] = States.Down;

			if (state == States.Released)
				this.mouseButtons[button] = States.Up;
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
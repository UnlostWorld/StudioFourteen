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

namespace StudioFourteen.Input.Devices;

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using StudioFourteen.Plugin;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using static StudioFourteen.Posing.RotationGizmo;
using InputKey = System.Windows.Input.Key;

public class KeyboardDevice : InputDeviceBase
{
	private readonly Dictionary<VirtualKey, KeyAxis> axisLookup = new();

	public KeyboardDevice()
	{
		if (DalamudServices.KeyState == null)
			return;

		foreach (VirtualKey key in DalamudServices.KeyState.GetValidVirtualKeys())
		{
			if (this.axisLookup.ContainsKey(key))
				continue;

			KeyAxis axis = new(key, KeyboardDevice.GetAxisId(key));
			this.axisLookup.Add(key, axis);
			this.Axes.Add(axis);
		}
	}

	public static IInputElement FocusedElement => System.Windows.Input.Keyboard.FocusedElement;

	public static string GetAxisId(VirtualKey key) => $"Keyboard:{key}";

	public override void PreUpdate()
	{
		if (DalamudServices.KeyState == null)
			return;

		if (this.Services.Input.IsXivTextInputActive)
			return;

		foreach (KeyAxis axis in this.Axes)
		{
			axis.IsConsumed = false;
		}

		if (this.Services.Panels.ActivePanel == null && this.Services.Windows.IsXivWindowActive())
		{
			// Read XIV -> Studio
			foreach(KeyAxis axis in this.Axes)
			{
				bool isDown = DalamudServices.KeyState[axis.VirtualKey];
				axis.Value = isDown ? 1.0f : 0.0f;
			}
		}
		else
		{
			// Write Studio -> XIV
			foreach (KeyAxis axis in this.Axes)
			{
				if (!DalamudServices.KeyState.IsVirtualKeyValid(axis.VirtualKey))
					continue;

				if (axis.State == InputService.States.Pressed)
				{
					this.Services.Windows.SendKeyToXiv(axis.VirtualKey, true);
				}
				else if (axis.State == InputService.States.Released)
				{
					this.Services.Windows.SendKeyToXiv(axis.VirtualKey, false);
				}
			}

			foreach (KeyAxis axis in this.Axes)
			{
				axis.AdvanceState();
			}
		}
	}

	public override void PostUpdate()
	{
		if (DalamudServices.KeyState == null)
			return;

		if (this.Services.Input.IsXivTextInputActive)
			return;

		if (this.Services.Panels.ActivePanel == null && this.Services.Windows.IsXivWindowActive())
		{
			// read XIV -> Studio
			foreach (KeyAxis axis in this.Axes)
			{
				if (axis.IsConsumed && DalamudServices.KeyState[axis.VirtualKey])
				{
					this.Services.Windows.ActivateStudioWindow();
					DalamudServices.KeyState[axis.VirtualKey] = false;
				}
			}
		}
		else
		{
			// Write studio -> XIV
			foreach (KeyAxis axis in this.Axes)
			{
				if (axis.IsConsumed)
				{
					DalamudServices.KeyState[axis.VirtualKey] = false;
				}
			}
		}
	}

	public IEnumerable<VirtualKey> GetValidKeys()
	{
		if (DalamudServices.KeyState == null)
			return new List<VirtualKey>();

		return DalamudServices.KeyState.GetValidVirtualKeys();
	}

	public void HandleKey(Key key, bool down)
	{
		VirtualKey vKey = (VirtualKey)KeyInterop.VirtualKeyFromKey(key);
		if (vKey == VirtualKey.NO_KEY)
			return;

		if (DalamudServices.KeyState == null)
			return;

		if (vKey == VirtualKey.LSHIFT || vKey == VirtualKey.RSHIFT)
			vKey = VirtualKey.SHIFT;

		if (vKey == VirtualKey.LMENU || vKey == VirtualKey.RMENU)
			vKey = VirtualKey.MENU;

		if (vKey == VirtualKey.LCONTROL || vKey == VirtualKey.RCONTROL)
			vKey = VirtualKey.CONTROL;

		if (!DalamudServices.KeyState.IsVirtualKeyValid(vKey))
			return;

		if (!this.axisLookup.ContainsKey(vKey))
			return;

		if (down)
		{
			if (this.axisLookup[vKey].State == InputService.States.Up)
			{
				this.axisLookup[vKey].Value = 1.0f;
			}
		}
		else
		{
			if (this.axisLookup[vKey].State == InputService.States.Down)
			{
				this.axisLookup[vKey].Value = 0.0f;
			}
		}

		if (down)
		{
			if (KeyboardDevice.FocusedElement is TextBoxBase tb)
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
}

public class KeyAxis : InputAxis
{
	public readonly VirtualKey VirtualKey;

	public KeyAxis(VirtualKey virtualKey, string id)
		: base(id)
	{
		this.VirtualKey = virtualKey;
	}

	public InputService.States State { get; private set; }

	public override float Value
	{
		get => base.Value;
		set
		{
			if (value > 0.5f)
			{
				switch (this.State)
				{
					case InputService.States.Released:
					case InputService.States.Up:
					{
						this.State = InputService.States.Pressed;
						break;
					}

					case InputService.States.Down:
					case InputService.States.Pressed:
					{
						this.State = InputService.States.Down;
						break;
					}
				}
			}
			else
			{
				switch (this.State)
				{
					case InputService.States.Down:
					case InputService.States.Pressed:
					{
						this.State = InputService.States.Released;
						break;
					}

					case InputService.States.Released:
					case InputService.States.Up:
					{
						this.State = InputService.States.Up;
						break;
					}
				}
			}

			base.Value = value;
		}
	}

	public void AdvanceState()
	{
		if (this.State == InputService.States.Pressed)
		{
			this.State = InputService.States.Down;
		}

		if (this.State == InputService.States.Released)
		{
			this.State = InputService.States.Up;
		}
	}
}
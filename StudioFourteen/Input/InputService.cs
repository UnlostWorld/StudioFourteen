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

using FFXIVClientStructs.FFXIV.Client.UI;
using Newtonsoft.Json;
using Serilog;
using StudioFourteen.Content;
using StudioFourteen.Input.Devices;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

public enum InputStates
{
	None,
	Activated,
	Held,
	Deactivated,
}

public class InputService : ServiceBase
{
	private readonly JsonContentReference<Dictionary<InputAction, List<Bind>>> defaultBinds = new("DefaultBinds.jsonc");

	private readonly Dictionary<InputAction, List<Input0DListener>> listeners = new();
	private readonly List<InputDeviceBase> inputDevices = new();
	private readonly Dictionary<string, InputAxis> axisLookup = new();
	private readonly List<Bind> binds = new();

	private readonly Input0DListener fastChangeListener = new(InputAction.FastChange, "Input Service Fast Change");
	private readonly Input0DListener slowChangeListener = new(InputAction.SlowChange, "Input Service Slow Change");

	private InputDeviceBase? currentDevice = null;

	public KeyboardDevice? Keyboard => this.GetDevice<KeyboardDevice>();
	public GamepadDevice? Gamepad => this.GetDevice<GamepadDevice>();
	public MouseDevice? Mouse => this.GetDevice<MouseDevice>();

	public bool IsXivTextInputActive { get; private set; }
	public bool IsStudioTextInputActive
	{
		get
		{
			if (KeyboardDevice.FocusedElement is TextBoxBase tb)
			{
				return tb.IsFocused && (tb.IsKeyboardFocused || tb.IsKeyboardFocusWithin);
			}

			return false;
		}
	}

	public bool SlowChange => this.slowChangeListener.Value > 0.05f;
	public bool FastChange => this.fastChangeListener.Value > 0.05f;

	public override Task Start()
	{
		this.AddDevice(new KeyboardDevice());
		this.AddDevice(new MouseDevice());
		this.AddDevice(new GamepadDevice());

		Dictionary<InputAction, List<Bind>> defaultBinds = this.defaultBinds.Get();
		foreach ((InputAction action, List<Bind> binds) in defaultBinds)
		{
			foreach (Bind bind in binds)
			{
				bind.Action = action;
				this.binds.Add(bind);
			}
		}

		// the order of binds controls the priority of execution,
		// binds earlier in the list will activate instead of ones lower
		// in the list.
		// sort the list so binds with more modifiers are on top
		// (so 'Shift+S' activates instead of 'S' when holding both)
		// And then sort by the action index (so navigation events will activate
		// instead of camera events if they both have listeners)
		this.binds.Sort((a, b) =>
		{
			if (a.ModifierAxes.Count > b.ModifierAxes.Count)
				return -1;

			if (a.ModifierAxes.Count < b.ModifierAxes.Count)
				return 1;

			if (a.Action < b.Action)
				return -1;

			if (a.Action > b.Action)
				return 1;

			return 0;
		});

		return base.Start();
	}

	public void AddDevice(InputDeviceBase device)
	{
		lock (this)
		{
			this.inputDevices.Add(device);

			foreach (InputAxis axis in device.Axes)
			{
				if (this.axisLookup.ContainsKey(axis.Id))
				{
					this.Log.Error($"Axis Id collision: {axis.Id}");
					continue;
				}

				this.axisLookup.Add(axis.Id, axis);
			}
		}
	}

	public TDevice? GetDevice<TDevice>()
		where TDevice : InputDeviceBase
	{
		foreach (InputDeviceBase device in this.inputDevices)
		{
			if (device is TDevice tDevice)
			{
				return tDevice;
			}
		}

		return null;
	}

	public bool HasListener(InputAction evt)
	{
		lock (this.listeners)
		{
			if (!this.listeners.ContainsKey(evt))
				return false;

			return this.listeners[evt].Count > 0;
		}
	}

	public void AddListener(InputAction evt, Input0DListener listener)
	{
		lock (this.listeners)
		{
			if (!this.listeners.ContainsKey(evt))
				this.listeners.Add(evt, new());

			this.listeners[evt].Add(listener);
		}
	}

	public void RemoveListener(InputAction evt, Input0DListener listener)
	{
		lock (this.listeners)
		{
			if (!this.listeners.ContainsKey(evt))
				return;

			this.listeners[evt].Remove(listener);
		}
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Add(TickService.Channels.LateGameTick, this.OnLateGameTick);

		foreach (InputDeviceBase device in this.inputDevices)
		{
			device.Attach();
		}

		this.slowChangeListener.Enable();
		this.fastChangeListener.Enable();

		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Remove(TickService.Channels.LateGameTick, this.OnLateGameTick);

		foreach (InputDeviceBase device in this.inputDevices)
		{
			device.Detach();
		}

		this.slowChangeListener.Disable();
		this.fastChangeListener.Disable();

		base.Detach();
	}

	public InputAxis? GetAxisId(string id)
	{
		if (this.axisLookup.TryGetValue(id, out InputAxis? value))
			return value;

		return null;
	}

	protected unsafe void OnGameTick()
	{
		if (!this.Services.Studio.IsOpen)
			return;

		lock (this)
		{
			foreach (InputDeviceBase device in this.inputDevices)
			{
				device.PreUpdate();
			}

			bool wasActive = this.IsXivTextInputActive;
			this.IsXivTextInputActive = RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

			// If Text Input just activated, and we have focus, set focus to xiv.
			/*if (!wasActive && this.IsXivTextInputActive && this.Services.Windows.IsAnyStudioWindowActive())
			{
				this.Services.Windows.ActivateXivWindow();
			}*/

			// If text input is still active, but we are taking focus, send the escape key to clear
			// the text input focus from xiv.
			// TODO: it would be nicer if we could invoke something in the AtkModule to clear the games input focus.
			/*if (wasActive && this.IsXivTextInputActive && this.Services.Windows.IsAnyStudioWindowActive())
			{
				this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, true);
				this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, false);
			}*/

			if (this.IsXivTextInputActive || this.IsStudioTextInputActive)
				return;

			Dictionary<InputAction, float> combinedValues = new();

			foreach (Bind bind in this.binds)
			{
				this.listeners.TryGetValue(bind.Action, out List<Input0DListener>? listeners);

				// nobody listening?
				if (listeners == null || listeners.Count == 0)
					continue;

				combinedValues.TryAdd(bind.Action, 0);
				combinedValues[bind.Action] += bind.GetValue();
			}

			foreach ((InputAction action, float value) in combinedValues)
			{
				this.listeners.TryGetValue(action, out List<Input0DListener>? listeners);

				if (listeners == null || listeners.Count == 0)
					continue;

				foreach (Input0DListener listener in listeners.ToArray())
				{
					listener.SetValue(value);
				}
			}
		}
	}

	protected unsafe void OnLateGameTick()
	{
		lock (this)
		{
			DateTime mostRecentInput = DateTime.MinValue;
			InputDeviceBase? mostRecentDevice = null;
			foreach (InputDeviceBase device in this.inputDevices)
			{
				device.PostUpdate();

				foreach (InputAxis axis in device.Axes)
				{
					if (!axis.CanActivateDevice)
						continue;

					if (axis.UtcLastInput > mostRecentInput)
					{
						mostRecentInput = axis.UtcLastInput;
						mostRecentDevice = device;
					}
				}
			}

			if (mostRecentDevice != this.currentDevice)
			{
				this.SetCurrentDevice(mostRecentDevice);
			}
		}
	}

	private void SetCurrentDevice(InputDeviceBase? device)
	{
		this.currentDevice?.Deactivate();
		this.currentDevice = device;
		this.currentDevice?.Activate();
	}
}

public class Bind
{
	[JsonIgnore] public ILogger Log => Logging.ForContext(this.GetType());
	[JsonIgnore] public ServiceManager Services => ServiceManager.Instance;

	public InputAction Action { get; set; }
	public string? PrimaryAxis { get; set; }
	public List<string> ModifierAxes { get; set; } = new();

	public float GetValue()
	{
		if (this.PrimaryAxis == null)
			return 0.0f;

		InputAxis? primaryAxis = this.Services.Input.GetAxisId(this.PrimaryAxis);
		if (primaryAxis == null || primaryAxis.IsConsumed)
			return 0.0f;

		float value = primaryAxis.Value;

		foreach (string axisId in this.ModifierAxes)
		{
			InputAxis? modifierAxis = this.Services.Input.GetAxisId(axisId);
			if (modifierAxis == null || modifierAxis.IsConsumed)
			{
				value = 0;
			}
			else
			{
				value *= modifierAxis.Value;
			}
		}

		value = Math.Max(value, 0);

		if (value > 0.001f)
		{
			primaryAxis.ConsumedBy = this;
		}

		return value;
	}
}
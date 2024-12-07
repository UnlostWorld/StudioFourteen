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
using Newtonsoft.Json;
using Serilog;
using StudioFourteen.Input.Devices;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

using MouseButtons = System.Windows.Input.MouseButton;

public class InputService : ServiceBase
{
	private readonly Dictionary<InputAction, List<InputActionListener>> listeners = new();
	private readonly List<InputDeviceBase> inputDevices = new();
	private readonly Dictionary<string, InputAxis> axisLookup = new();
	private readonly List<Bind> binds = new();

	private InputDeviceBase? currentDevice = null;

	public InputService()
	{
		// Navigation
		this.AddBind(InputAction.Navigate_Up, KeyboardDevice.GetAxisId(VirtualKey.UP));
		this.AddBind(InputAction.Navigate_Down, KeyboardDevice.GetAxisId(VirtualKey.DOWN));
		this.AddBind(InputAction.Navigate_Left, KeyboardDevice.GetAxisId(VirtualKey.LEFT));
		this.AddBind(InputAction.Navigate_Right, KeyboardDevice.GetAxisId(VirtualKey.RIGHT));
		this.AddBind(InputAction.Navigate_Enter, KeyboardDevice.GetAxisId(VirtualKey.RETURN));

		this.AddBind(InputAction.Navigate_Up, KeyboardDevice.GetAxisId(VirtualKey.W));
		this.AddBind(InputAction.Navigate_Down, KeyboardDevice.GetAxisId(VirtualKey.S));
		this.AddBind(InputAction.Navigate_Left, KeyboardDevice.GetAxisId(VirtualKey.A));
		this.AddBind(InputAction.Navigate_Right, KeyboardDevice.GetAxisId(VirtualKey.D));
		this.AddBind(InputAction.Navigate_TabLeft, KeyboardDevice.GetAxisId(VirtualKey.Q));
		this.AddBind(InputAction.Navigate_TabRight, KeyboardDevice.GetAxisId(VirtualKey.E));
		this.AddBind(InputAction.Navigate_Enter, KeyboardDevice.GetAxisId(VirtualKey.SPACE));
		this.AddBind(InputAction.Navigate_Back, KeyboardDevice.GetAxisId(VirtualKey.ESCAPE));

		this.AddBind(InputAction.Navigate_Up, GamepadDevice.GetAxisId(GamepadDevice.Buttons.DpadUp));
		this.AddBind(InputAction.Navigate_Down, GamepadDevice.GetAxisId(GamepadDevice.Buttons.DpadDown));
		this.AddBind(InputAction.Navigate_Left, GamepadDevice.GetAxisId(GamepadDevice.Buttons.DpadLeft));
		this.AddBind(InputAction.Navigate_Right, GamepadDevice.GetAxisId(GamepadDevice.Buttons.DpadRight));
		this.AddBind(InputAction.Navigate_TabLeft, GamepadDevice.GetAxisId(GamepadDevice.Buttons.LeftShoulder));
		this.AddBind(InputAction.Navigate_TabRight, GamepadDevice.GetAxisId(GamepadDevice.Buttons.RightShoulder));
		this.AddBind(InputAction.Navigate_Enter, GamepadDevice.GetAxisId(GamepadDevice.Buttons.FaceDown));
		this.AddBind(InputAction.Navigate_Back, GamepadDevice.GetAxisId(GamepadDevice.Buttons.FaceRight));

		// General
		this.AddBind(InputAction.InvokeQuickSearch, KeyboardDevice.GetAxisId(VirtualKey.Q), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.Save, KeyboardDevice.GetAxisId(VirtualKey.S), KeyboardDevice.GetAxisId(VirtualKey.CONTROL));
		this.AddBind(InputAction.SaveAs, KeyboardDevice.GetAxisId(VirtualKey.S), KeyboardDevice.GetAxisId(VirtualKey.CONTROL), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));

		// Free Camera
		this.AddBind(InputAction.FreeCamera_MoveForwards, KeyboardDevice.GetAxisId(VirtualKey.W));
		this.AddBind(InputAction.FreeCamera_MoveBack, KeyboardDevice.GetAxisId(VirtualKey.S));
		this.AddBind(InputAction.FreeCamera_MoveLeft, KeyboardDevice.GetAxisId(VirtualKey.A));
		this.AddBind(InputAction.FreeCamera_MoveRight, KeyboardDevice.GetAxisId(VirtualKey.D));
		this.AddBind(InputAction.FreeCamera_MoveUp, KeyboardDevice.GetAxisId(VirtualKey.Q));
		this.AddBind(InputAction.FreeCamera_MoveDown, KeyboardDevice.GetAxisId(VirtualKey.E));
		this.AddBind(InputAction.FreeCamera_YawLeft, KeyboardDevice.GetAxisId(VirtualKey.A), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_YawRight, KeyboardDevice.GetAxisId(VirtualKey.D), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_PitchUp, KeyboardDevice.GetAxisId(VirtualKey.W), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_PitchDown, KeyboardDevice.GetAxisId(VirtualKey.S), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_RollLeft, KeyboardDevice.GetAxisId(VirtualKey.Q), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_RollRight, KeyboardDevice.GetAxisId(VirtualKey.E), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.FreeCamera_RotateRight, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Right));
		this.AddBind(InputAction.FreeCamera_RotateLeft, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Left));
		this.AddBind(InputAction.FreeCamera_RotateDown, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Down));
		this.AddBind(InputAction.FreeCamera_RotateUp, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Up));

		// Orbit Camera
		this.AddBind(InputAction.OrbitCamera_PanUp, KeyboardDevice.GetAxisId(VirtualKey.W));
		this.AddBind(InputAction.OrbitCamera_PanDown, KeyboardDevice.GetAxisId(VirtualKey.S));
		this.AddBind(InputAction.OrbitCamera_PanLeft, KeyboardDevice.GetAxisId(VirtualKey.A));
		this.AddBind(InputAction.OrbitCamera_PanRight, KeyboardDevice.GetAxisId(VirtualKey.D));
		this.AddBind(InputAction.OrbitCamera_PanRight, MouseDevice.GetDragAxisId(MouseButtons.Middle, MouseDevice.DragDirections.Right));
		this.AddBind(InputAction.OrbitCamera_PanLeft, MouseDevice.GetDragAxisId(MouseButtons.Middle, MouseDevice.DragDirections.Left));
		this.AddBind(InputAction.OrbitCamera_PanDown, MouseDevice.GetDragAxisId(MouseButtons.Middle, MouseDevice.DragDirections.Down));
		this.AddBind(InputAction.OrbitCamera_PanUp, MouseDevice.GetDragAxisId(MouseButtons.Middle, MouseDevice.DragDirections.Up));
		this.AddBind(InputAction.OrbitCamera_RollLeft, KeyboardDevice.GetAxisId(VirtualKey.Q));
		this.AddBind(InputAction.OrbitCamera_RollRight, KeyboardDevice.GetAxisId(VirtualKey.E));
		this.AddBind(InputAction.OrbitCamera_MoveUp, KeyboardDevice.GetAxisId(VirtualKey.W), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.OrbitCamera_MoveDown, KeyboardDevice.GetAxisId(VirtualKey.S), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.OrbitCamera_MoveLeft, KeyboardDevice.GetAxisId(VirtualKey.A), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.OrbitCamera_MoveRight, KeyboardDevice.GetAxisId(VirtualKey.D), KeyboardDevice.GetAxisId(VirtualKey.SHIFT));
		this.AddBind(InputAction.OrbitCamera_MoveUp, MouseDevice.GetDragAxisId(MouseButtons.Right, MouseDevice.DragDirections.Up));
		this.AddBind(InputAction.OrbitCamera_MoveDown, MouseDevice.GetDragAxisId(MouseButtons.Right, MouseDevice.DragDirections.Down));
		this.AddBind(InputAction.OrbitCamera_MoveLeft, MouseDevice.GetDragAxisId(MouseButtons.Right, MouseDevice.DragDirections.Left));
		this.AddBind(InputAction.OrbitCamera_MoveRight, MouseDevice.GetDragAxisId(MouseButtons.Right, MouseDevice.DragDirections.Right));
		this.AddBind(InputAction.OrbitCamera_ZoomIn, MouseDevice.WheelPos);
		this.AddBind(InputAction.OrbitCamera_ZoomOut, MouseDevice.WheelNeg);
		this.AddBind(InputAction.OrbitCamera_RotateRight, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Right));
		this.AddBind(InputAction.OrbitCamera_RotateLeft, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Left));
		this.AddBind(InputAction.OrbitCamera_RotateDown, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Down));
		this.AddBind(InputAction.OrbitCamera_RotateUp, MouseDevice.GetDragAxisId(MouseButtons.Left, MouseDevice.DragDirections.Up));
	}

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

	public override Task Start()
	{
		this.AddDevice(new KeyboardDevice());
		this.AddDevice(new MouseDevice());
		this.AddDevice(new GamepadDevice());

		return base.Start();
	}

	public void AddDevice(InputDeviceBase device)
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

	public void AddBind(InputAction action, string primaryAxisId, params string[] modifierAxisIds)
	{
		Bind bind = new();
		bind.Action = action;
		bind.PrimaryAxis = primaryAxisId;
		bind.ModifierAxes.AddRange(modifierAxisIds);
		this.binds.Add(bind);

		// the order of binds controls the priority of execution,
		// binds earlier in teh list will activate instead of ones lower
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
	}

	public bool HasListener(InputAction evt)
	{
		if (!this.listeners.ContainsKey(evt))
			return false;

		return this.listeners[evt].Count > 0;
	}

	public void AddListener(InputAction evt, InputActionListener listener)
	{
		if (!this.listeners.ContainsKey(evt))
			this.listeners.Add(evt, new());

		this.listeners[evt].Add(listener);
	}

	public void RemoveListener(InputAction evt, InputActionListener listener)
	{
		if (!this.listeners.ContainsKey(evt))
			return;

		this.listeners[evt].Remove(listener);
	}

	public override void Attach()
	{
		foreach(InputDeviceBase device in this.inputDevices)
		{
			device.Attach();
		}

		base.Attach();
	}

	public override void Detach()
	{
		foreach (InputDeviceBase device in this.inputDevices)
		{
			device.Detach();
		}

		base.Detach();
	}

	public InputAxis? GetAxisId(string id)
	{
		if (this.axisLookup.TryGetValue(id, out InputAxis? value))
			return value;

		return null;
	}

	protected override unsafe void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (!this.Services.Studio.IsOpen)
			return;

		foreach (InputDeviceBase device in this.inputDevices)
		{
			device.PreUpdate();
		}

		bool wasActive = this.IsXivTextInputActive;
		this.IsXivTextInputActive = RaptureAtkModule.Instance()->AtkModule.IsTextInputActive();

		// If Text Input just activated, and we have focus, set focus to xiv.
		if (!wasActive && this.IsXivTextInputActive && this.Services.Windows.IsAnyStudioWindowActive())
		{
			this.Services.Windows.ActivateXivWindow();
		}

		// If text input is still active, but we are taking focus, send the escape key to clear
		// the text input focus from xiv.
		// TODO: it would be nicer if we could invoke something in the AtkModule to clear the games input focus.
		if (wasActive && this.IsXivTextInputActive && this.Services.Windows.IsAnyStudioWindowActive())
		{
			this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, true);
			this.Services.Windows.SendKeyToXiv(VirtualKey.ESCAPE, false);
		}

		if (this.IsXivTextInputActive || this.IsStudioTextInputActive)
			return;

		Dictionary<InputAction, float> combinedValues = new();

		foreach(Bind bind in this.binds)
		{
			this.listeners.TryGetValue(bind.Action, out List<InputActionListener>? listeners);

			// nobody listening?
			if (listeners == null || listeners.Count == 0)
				continue;

			combinedValues.TryAdd(bind.Action, 0);
			combinedValues[bind.Action] += bind.GetValue();
		}

		foreach((InputAction action, float value) in combinedValues)
		{
			this.listeners.TryGetValue(action, out List<InputActionListener>? listeners);

			if (listeners == null || listeners.Count == 0)
				continue;

			foreach (InputActionListener listener in listeners)
			{
				listener.SetValue(value);
			}
		}

		DateTime mostRecentInput = DateTime.MinValue;
		InputDeviceBase? mostRecentDevice = null;
		foreach (InputDeviceBase device in this.inputDevices)
		{
			device.PostUpdate();

			foreach(InputAxis axis in device.Axes)
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

	public int Priority { get; set; }
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
			primaryAxis.IsConsumed = true;

		return value;
	}
}
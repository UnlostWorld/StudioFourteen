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
using Serilog;
using System;

public class InputActionListener
{
	public readonly ILogger Log = Logging.ForContext<InputActionListener>();

	private readonly InputAction keyBindEvent;
	private InputService.States currentState = InputService.States.Up;
	private InputService.States cacheState = InputService.States.Up;

	public InputActionListener(InputAction evt)
	{
		this.keyBindEvent = evt;
	}

	public Action? Pressed { get; set; }
	public Action? Down { get; set; }
	public Action? Released { get; set; }
	public float Value { get; set; }

	public void Enable()
	{
		ServiceManager.Instance.Input.AddListener(this.keyBindEvent, this);
	}

	public void Disable()
	{
		ServiceManager.Instance.Input.RemoveListener(this.keyBindEvent, this);
	}

	public void SetValue(float axisValue)
	{
		this.Value = axisValue;

		InputService.States state = axisValue > 0 ? InputService.States.Down : InputService.States.Up;

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
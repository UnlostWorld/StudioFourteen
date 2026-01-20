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

namespace StudioFourteen.Services.Input;

using Serilog;
using System;

public class Input0DListener
{
	private readonly InputAction keyBindEvent;
	private float lastValue = 0;

	public Input0DListener(InputAction evt, string? name = null)
	{
		this.keyBindEvent = evt;
		this.Name = name;
	}

	public float Value { get; set; }
	public Action? Activate { get; set; }
	public Action? Deactivate { get; set; }
	public string? Name { get; init; }

	public void Enable()
	{
		Studio.Input.AddListener(this.keyBindEvent, this);
	}

	public void Disable()
	{
		if (Studio.IsDisposed)
			return;

		Studio.Input.RemoveListener(this.keyBindEvent, this);
	}

	public void SetValue(float newValue)
	{
		float oldValue = this.Value;
		this.Value = newValue;

		try
		{
			if (oldValue < 0.001f && newValue > 0.001f)
			{
				this.Activate?.Invoke();
			}
			else if (oldValue > 0.001f && newValue < 0.001f)
			{
				this.Deactivate?.Invoke();
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, $"Error invoking key bind callback for event {this.keyBindEvent}");
		}
	}

	public InputStates GetState()
	{
		float oldValue = this.lastValue;
		this.lastValue = this.Value;

		if (oldValue < 0.001f && this.Value > 0.001f)
		{
			return InputStates.Activated;
		}
		else if (oldValue > 0.001f && this.Value < 0.001f)
		{
			return InputStates.Deactivated;
		}
		else if (oldValue > 0.001 && this.Value > 0.001f)
		{
			return InputStates.Held;
		}

		return InputStates.None;
	}
}
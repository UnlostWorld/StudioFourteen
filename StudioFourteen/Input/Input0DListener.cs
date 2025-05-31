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

public class Input0DListener
{
	public readonly ILogger Log = Logging.ForContext<Input0DListener>();

	private readonly InputAction keyBindEvent;

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
		ServiceManager.Instance.Input.AddListener(this.keyBindEvent, this);
	}

	public void Disable()
	{
		if (ServiceManager.ShutdownRequested)
			return;

		ServiceManager.Instance.Input.RemoveListener(this.keyBindEvent, this);
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
			this.Log.Error(ex, $"Error invoking key bind callback for event {this.keyBindEvent}");
		}
	}
}
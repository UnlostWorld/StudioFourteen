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

using Serilog;
using System;
using System.Collections.Generic;

public abstract class InputDeviceBase
{
	public List<InputAxis> Axes { get; init; } = new();

	protected ILogger Log => Logging.ForContext(this.GetType());
	protected ServiceManager Services => ServiceManager.Instance;

	// Called when Studio attaches to XIV.
	public virtual void Attach()
	{
	}

	// Called when Studio detaches from XIV.
	public virtual void Detach()
	{
	}

	// Called when this device becomes primary, by being the most recent
	// device to send inputs.
	public virtual void Activate()
	{
	}

	// Called when this device is no longer primary.
	public virtual void Deactivate()
	{
	}

	// Called at the start of Framework Update, before binds have been polled.
	// Typically reset InputAxis.IsConsumed here.
	public virtual void PreUpdate()
	{
	}

	// Called after all binds have been polled.
	// Typically check InputAxis.IsConsumed here, and forward events to XIV.
	public virtual void PostUpdate()
	{
	}

	protected void AddAxis(InputAxis axis)
	{
		this.Axes.Add(axis);
	}

	protected void AddAxis(InputAxisSigned axis)
	{
		this.AddAxis(axis.Positive);
		this.AddAxis(axis.Negative);
	}
}

public class InputAxis(string id, InputDeviceBase device, bool canActivateDevice)
{
	private float value;

	public string Id => id;
	public InputDeviceBase Device => device;
	public bool IsConsumed { get; set; }

	public DateTime UtcLastInput { get; private set; }
	public bool CanActivateDevice => canActivateDevice;

	public virtual float Value
	{
		get => this.value;
		set
		{
			this.value = value;

			if (value > 0.001)
			{
				this.UtcLastInput = DateTime.UtcNow;
			}
		}
	}
}

public class InputAxisSigned(string positiveId, string negativeId, InputDeviceBase device, bool canActivateDevice)
{
	public readonly InputAxis Positive = new(positiveId, device, canActivateDevice);
	public readonly InputAxis Negative = new(negativeId, device, canActivateDevice);

	public bool IsConsumed
	{
		get => this.Positive.IsConsumed || this.Negative.IsConsumed;
		set
		{
			this.Positive.IsConsumed = value;
			this.Negative.IsConsumed = value;
		}
	}

	public float Value
	{
		get => this.Positive.Value - this.Negative.Value;
		set
		{
			this.Positive.Value = 0;
			this.Negative.Value = 0;

			if (value > 0)
			{
				this.Positive.Value = value;
			}
			else if (value < 0)
			{
				this.Negative.Value = -value;
			}
		}
	}
}
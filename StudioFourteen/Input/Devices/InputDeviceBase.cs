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
using System.Collections.Generic;

public abstract class InputDeviceBase
{
	public List<InputAxis> Axes { get; init; } = new();

	protected ILogger Log => Logging.ForContext(this.GetType());
	protected ServiceManager Services => ServiceManager.Instance;

	public virtual void Attach()
	{
	}

	public virtual void Detach()
	{
	}

	public virtual void PreUpdate()
	{
	}

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

public class InputAxis(string id)
{
	public string Id => id;
	public bool IsConsumed { get; set; }
	public virtual float Value { get; set; }
}

public class InputAxisSigned(string positiveId, string negativeId)
{
	public readonly InputAxis Positive = new(positiveId);
	public readonly InputAxis Negative = new(negativeId);

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
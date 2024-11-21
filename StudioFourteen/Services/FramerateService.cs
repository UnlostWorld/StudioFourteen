// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;

public class FramerateService : ServiceBase
{
	private readonly float[] frameTimes = new float[5];
	private int currentFrameIndex = 0;

	public static float AverageDeltaTime { get; private set; }

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
		this.currentFrameIndex++;

		if (this.currentFrameIndex >= this.frameTimes.Length)
			this.currentFrameIndex = 0;

		this.frameTimes[this.currentFrameIndex] = (float)framework.UpdateDelta.TotalSeconds;

		float totalTime = 0;
		for (int i = 0; i < this.frameTimes.Length; i++)
		{
			totalTime += this.frameTimes[i];
		}

		AverageDeltaTime = totalTime / this.frameTimes.Length;
	}
}

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

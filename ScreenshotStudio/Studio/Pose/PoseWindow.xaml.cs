namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;

public partial class PoseWindow : ActorWindow
{
	[AutoNotify]
	public bool ExpandTranslationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandRotationSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	[AutoNotify]
	public bool ExpandScaleSliders
	{
		get => this.GetPersistence<bool>();
		set => this.SetPersistence(value);
	}

	public double TranslationX { get; set; }
	public double TranslationY { get; set; }
	public double TranslationZ { get; set; }

	public double EulerRotationX { get; set; }
	public double EulerRotationY { get; set; }
	public double EulerRotationZ { get; set; }

	public double ScaleX { get; set; }
	public double ScaleY { get; set; }
	public double ScaleZ { get; set; }
}
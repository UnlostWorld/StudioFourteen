namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Windows;

[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class ExpressionsView : View
{
	private BlendSelection? mouthSelection;

	public double BackgroundOpacity => SkeletonView.BackgroundOpacity;

	private void OnMouthClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.mouthSelection;
	}

	private void OnLeftEyeClicked(object sender, RoutedEventArgs e)
	{
	}

	private void OnRightEyeClicked(object sender, RoutedEventArgs e)
	{
	}

	partial void OnObjectTableIndexChanged(int newValue)
	{
		if (this.Services.Blend.MouthSource != null)
			this.mouthSelection = new(this.Services.Blend.MouthSource);

		////this.leftEyeSelection = new(this.Services.Blend.MouthSource);
		////this.rightEyeSelection = new(this.Services.Blend.MouthSource);
	}
}
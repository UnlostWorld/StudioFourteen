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
		if (this.Services.Data.ExpressionBlends == null)
			return;

		this.mouthSelection = new("Mouth", new()
		{
			this.Services.Data.ExpressionBlends["MouthFrown"],
			this.Services.Data.ExpressionBlends["MouthGrin"],
			this.Services.Data.ExpressionBlends["MouthPucker"],
			this.Services.Data.ExpressionBlends["MouthScared"],
			this.Services.Data.ExpressionBlends["MouthSmile"],
			this.Services.Data.ExpressionBlends["TongueOut"]
		});
	}
}
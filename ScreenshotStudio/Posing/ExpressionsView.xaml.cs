namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Numerics;
using System.Windows;

[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class ExpressionsView : View
{
	private ExpressionComponentSelection? mouthSelection;
	private ExpressionComponentSelection? leftEyeSelection;
	private ExpressionComponentSelection? rightEyeSelection;

	public double BackgroundOpacity => SkeletonView.BackgroundOpacity;

	private void OnMouthClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.mouthSelection;
	}

	private void OnLeftEyeClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.leftEyeSelection;
	}

	private void OnRightEyeClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.rightEyeSelection;
	}

	partial void OnObjectTableIndexChanged(int newValue)
	{
		this.mouthSelection = new("Mouth");
		this.leftEyeSelection = new("Left Eye");
		this.rightEyeSelection = new("Right Eye");
	}
}

public class ExpressionComponentSelection(string name)
	: SelectionBase
{
	public override string Name => name;
	public override string? Subtitle => null;
}
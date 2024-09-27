namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;

[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class ExpressionsView : View
{
	public ExpressionsView()
	{
		this.Services.Pose.SelectionChanged += this.OnPoseSelectionChanged;
	}

	public double BackgroundOpacity => SkeletonView.BackgroundOpacity;

	private void OnPoseSelectionChanged(SelectionBase? newSelection)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.MouthToggle.IsChecked = false;
			this.LeftEyeToggle.IsChecked = false;
			this.RightEyeToggle.IsChecked = false;
		});
	}

	partial void OnObjectTableIndexChanged(int newValue)
	{
	}
}
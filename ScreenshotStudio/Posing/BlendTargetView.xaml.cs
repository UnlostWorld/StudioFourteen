namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Windows;

[DependencyProperty<string>("TargetName")]
[DependencyProperty<bool>("Flip")]
public partial class BlendTargetView : View
{
	[AutoNotify] public BlendTarget? Target { get; private set; }

	partial void OnTargetNameChanged()
	{
		if (this.Services.Data.ExpressionBlends == null)
			return;

		if (this.TargetName == null)
			return;

		BlendTarget? target;
		this.Services.Data.ExpressionBlends.TryGetValue(this.TargetName, out target);

		this.Target = target;
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.TargetName == null || this.Target == null)
			return;

		BlendSelection selection = new(this.TargetName, this.Target);
		selection.IsFlipped = this.Flip;
		this.Services.Pose.Selection = selection;
	}
}

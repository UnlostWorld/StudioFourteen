namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Windows;

[DependencyProperty<BlendSelection>("Selection")]
public partial class BlendInspector : View
{
	public PoseWindow? Panel => this.FindParent<PoseWindow>();

	[AutoNotify] public double BlendMaximum => (this.Selection?.Maximum * 100) ?? 100;
	[AutoNotify] public double BlendMinimum => (this.Selection?.Minimum * 100) ?? 0;

	[AutoNotify]
	public double BlendValue
	{
		get => (this.Selection?.Value * 100) ?? 0;
		set => this.Selection?.SetValue(value / 100);
	}
}
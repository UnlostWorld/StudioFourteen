namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Mvm;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Extensions;

[DependencyProperty<BlendSelection>("Selection")]
public partial class BlendInspector : View
{
	public PoseWindow? Panel => this.FindParent<PoseWindow>();

	[AutoNotify] public double BlendMaximum => (this.ActiveBlend?.Maximum * 100) ?? 100;
	[AutoNotify] public double BlendMinimum => (this.ActiveBlend?.Minimum * 100) ?? 0;
	[AutoNotify] public BlendService.Blend? ActiveBlend { get; private set; }

	[AutoNotify]
	public double BlendValue
	{
		get => (this.ActiveBlend?.Value * 100) ?? 0;
		set => this.ActiveBlend?.SetValue(value / 100);
	}

	partial void OnSelectionChanged()
	{
		this.Begin().Run();
	}

	private async Task Begin()
	{
		if (this.Selection == null)
			return;

		this.ActiveBlend = await this.Services.Blend.BeginBlend(this.Services.Target.TargetObjectIndex, this.Selection.Target);
	}
}
namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Files;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Utilities;
using System.Windows;

[DependencyProperty<BlendSelection>("Selection")]
public partial class BlendInspector : View
{
	private BlendService.Blend? activeBlend;

	public PoseWindow? Panel => this.FindParent<PoseWindow>();

	[AutoNotify] public FrameworkElement? BlendTargetElement { get; set; }
	[AutoNotify] public bool IsBlendOpen
	{
		get => this.activeBlend != null;
		set
		{
			if (this.activeBlend == null)
				return;

			this.activeBlend = value ? this.activeBlend : null;
		}
	}

	[AutoNotify]
	public double BlendValue
	{
		get => this.activeBlend?.Value ?? 0;
		set => this.activeBlend?.SetValue(value);
	}

	private async void OnTargetClicked(object sender, RoutedEventArgs e)
	{
		this.activeBlend = null;

		this.BlendTargetElement = sender as FrameworkElement;

		FileEntry? fileEntry = this.BlendTargetElement?.DataContext as FileEntry;
		if (fileEntry == null)
			return;

		PoseFile? target = fileEntry.File as PoseFile;
		if (target == null)
			return;

		this.activeBlend = await this.Services.Blend.BeginBlend(this.Services.Target.TargetObjectIndex, target);
	}
}
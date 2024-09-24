namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Mvm;
using System.Collections.Generic;
using System.Windows;

[DependencyProperty<BlendSelection>("Selection")]
public partial class BlendInspector : View
{
	public PoseWindow? Panel => this.FindParent<PoseWindow>();

	[AutoNotify] public FrameworkElement? BlendTargetElement { get; set; }
	[AutoNotify] public bool IsBlendOpen { get; set; }

	private void OnTargetClicked(object sender, RoutedEventArgs e)
	{
		this.BlendTargetElement = sender as FrameworkElement;
		this.IsBlendOpen = true;
	}
}

public class BlendSelection(string name)
	: SelectionBase
{
	public override string Name => name;
	public override string? Subtitle => null;

	public List<FileEntry> BlendTargets { get; init; } = new();
}
// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Windows;

public partial class GearWindow : PanelWindow
{
	[AutoNotify]
	public unsafe float ModelHeight
	{
		get => this.Target->Model->Height;
		set => this.Target->Model->Height = value;
	}
}
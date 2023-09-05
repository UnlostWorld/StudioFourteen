// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio;

using ScreenshotStudio.Services;
using ScreenshotStudio.Studio.Pose;
using ScreenshotStudio.Windows;

public partial class PoseWindow : ActorWindow
{
	[AutoNotify]
	public bool IsBonesWindowOpen
	{
		get => this.Services.Panels.GetIsOpen<BoneWindow>();
		set => this.Services.Panels.SetIsOpen<BoneWindow>(value);
	}
}
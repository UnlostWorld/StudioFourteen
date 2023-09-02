// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;

public abstract class ActorWindow : PanelWindow
{
	[AlwaysNotify] public unsafe bool HasValidTarget => this.Actor != null;
	[AlwaysNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";

	// TODO: outside of gpose.
	protected unsafe Actor* Actor => this.Services.Targets.GPoseTarget;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}
}

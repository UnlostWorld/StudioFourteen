// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Windows;

using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;

public abstract class ActorWindow : PanelWindow
{
	[AlwaysNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";

	[AlwaysNotify] public unsafe bool HasValidTarget
	{
		get
		{
			if (this.Actor == null)
				return false;

			if (this.Actor->RenderMode != RenderMode.Draw)
				return false;

			return true;
		}
	}

	/// <summary>
	///  Gets a pointer to the player, the players target, or the gpose target.
	/// </summary>
	public unsafe Actor* Actor
	{
		get
		{
			if (DalamudServices.PluginInterface.UiBuilder.GposeActive)
			{
				// GPose target
				return (Actor*)TargetSystem.Instance()->GPoseTarget;
			}
			else
			{
				// Focus Target
				GameObject* pTargetObject = TargetSystem.Instance()->FocusTarget;
				if (pTargetObject != null && pTargetObject->IsCharacter())
				{
					return (Actor*)pTargetObject;
				}

				// Target
				pTargetObject = TargetSystem.Instance()->Target;
				if (pTargetObject != null && pTargetObject->IsCharacter())
				{
					return (Actor*)pTargetObject;
				}

				// Player
				return (Actor*)DalamudServices.ObjectTable.GetObjectAddress(0);
			}
		}
	}

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}
}

namespace ScreenshotStudio.Windows;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;

public abstract class ActorWindow : PanelWindow
{
	[AlwaysNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";

	[AlwaysNotify]
	public unsafe bool HasValidTarget
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
	///  Gets a pointer to the player, the players target, or the group pose target.
	/// </summary>
	public unsafe Actor* Actor { get; private set; }

	/// <summary>
	/// Gets the DrawData for the current Actor.
	/// </summary>
	public unsafe ActorDrawData DrawData => this.Actor->DrawData;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (DalamudServices.ObjectTable == null)
			return;

		if (GroupPoseService.IsGroupPosing)
		{
			// GPose target
			this.Actor = (Actor*)TargetSystem.Instance()->GPoseTarget;
		}
		else
		{
			// Focus Target
			GameObject* pTargetObject = TargetSystem.Instance()->FocusTarget;
			if (pTargetObject != null && pTargetObject->IsCharacter())
			{
				this.Actor = (Actor*)pTargetObject;
				return;
			}

			// Target
			pTargetObject = TargetSystem.Instance()->Target;
			if (pTargetObject != null && pTargetObject->IsCharacter())
			{
				this.Actor = (Actor*)pTargetObject;
				return;
			}

			// Player
			this.Actor = (Actor*)DalamudServices.ObjectTable.GetObjectAddress(0);
		}
	}
}
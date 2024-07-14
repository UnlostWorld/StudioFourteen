namespace ScreenshotStudio.Windows;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Utilities;
using System;

public abstract class ActorWindow : PanelWindow
{
	[AlwaysNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->Name : "Nobody";

	[AlwaysNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			try
			{
				if (this.Actor == null)
					return false;

				if (this.Actor->RenderMode != RenderMode.Draw)
					return false;

				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}

	/// <summary>
	///  Gets a pointer to the player, the players target, or the group pose target.
	/// </summary>
	public unsafe Actor* Actor { get; private set; }

	/// <summary>
	/// Gets the DrawData for the current Actor.
	/// </summary>
	public unsafe ref ActorDrawData DrawData => ref this.Actor->DrawData;

	// should put this somewhere...
	public static unsafe Actor* GetTarget()
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		if (GroupPoseService.IsGroupPosing)
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

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
		this.Actor = GetTarget();
	}
}
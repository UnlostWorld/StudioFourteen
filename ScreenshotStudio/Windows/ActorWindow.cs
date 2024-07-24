namespace ScreenshotStudio.Windows;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Utilities;
using System;

public abstract class ActorWindow : PanelWindow
{
	[AlwaysNotify] public unsafe string? ActorName => this.HasValidTarget ? this.Actor->GetNameAsString() : "Nobody";

	[AlwaysNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			try
			{
				if (this.Actor == null)
					return false;

				if (this.Actor->GameObject.RenderFlags != (int)RenderMode.Draw)
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
	public unsafe Character* Actor { get; private set; }

	/// <summary>
	/// Gets the DrawData for the current Actor.
	/// </summary>
	public unsafe ref DrawDataContainer DrawData => ref this.Actor->DrawData;

	// should put this somewhere...
	public static unsafe Character* GetTarget()
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		if (ServiceManager.Instance.GroupPose.IsGroupPosing)
		{
			// GPose target
			return (Character*)TargetSystem.Instance()->GPoseTarget;
		}
		else
		{
			// Focus Target
			GameObject* pTargetObject = TargetSystem.Instance()->FocusTarget;
			if (pTargetObject != null && pTargetObject->IsCharacter())
			{
				return (Character*)pTargetObject;
			}

			// Target
			pTargetObject = TargetSystem.Instance()->Target;
			if (pTargetObject != null && pTargetObject->IsCharacter())
			{
				return (Character*)pTargetObject;
			}

			// Player
			return (Character*)DalamudServices.ObjectTable.GetObjectAddress(0);
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
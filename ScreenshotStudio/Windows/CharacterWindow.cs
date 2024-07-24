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

public abstract class CharacterWindow : PanelWindow
{
	[AlwaysNotify] public unsafe string? CharacterName => this.HasValidTarget ? this.Target->GetNameAsString() : "Nobody";

	[AlwaysNotify]
	public unsafe bool HasValidTarget
	{
		get
		{
			try
			{
				if (this.Target == null)
					return false;

				if (this.Target->RenderFlags != (int)RenderMode.Draw)
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
	public unsafe Character* Target { get; private set; }

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
		this.Target = GetTarget();
	}
}
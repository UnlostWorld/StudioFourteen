namespace ScreenshotStudio.Windows;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;

public abstract class CharacterWindow : PanelWindow
{
	[AlwaysNotify] public string? CharacterName { get; private set; }
	[AlwaysNotify] public bool HasValidTarget { get; private set; }
	[AlwaysNotify] public int TargetObjectIndex { get; private set; }

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
		this.HasValidTarget = this.Target != null && this.Target->CanDraw();
		this.TargetObjectIndex = this.HasValidTarget ? this.Target->ObjectIndex : -1;
		this.CharacterName = this.HasValidTarget ? this.Target->GetNameAsString() : "Nobody";
	}
}
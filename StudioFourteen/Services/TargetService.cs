namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;

public class TargetService : ServiceBase
{
	public delegate void TargetChangedDelegate();

	public event TargetChangedDelegate? TargetChanged;

	[AlwaysNotify] public string? CharacterName { get; private set; }
	[AlwaysNotify] public bool HasValidTarget { get; private set; } = false;
	[AlwaysNotify] public bool IsTargetLoading { get; private set; } = false;
	[AlwaysNotify] public int TargetObjectIndex { get; private set; } = -1;

	/// <summary>
	///  Gets a pointer to the player, the players target, or the group pose target.
	///  Use with caution, as this pointer may not be safe after FrameworkUpdates.
	/// </summary>
	public unsafe Character* Target { get; private set; } = null;

	public unsafe void SetTarget(int objectTableIndex)
	{
		Threads.RunOnFrameworkThread(() =>
		{
			if (DalamudServices.ObjectTable == null)
				return;

			GameObject* target = (GameObject*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);

			if (target == null)
				return;

			TargetSystem.Instance()->GPoseTarget = target;
		});
	}

	public unsafe Character* GetTarget()
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

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		int startIndex = this.TargetObjectIndex;

		Character* pTarget = this.GetTarget();
		this.HasValidTarget = pTarget != null && pTarget->CanDraw();
		this.TargetObjectIndex = this.HasValidTarget ? pTarget->ObjectIndex : -1;
		this.CharacterName = this.HasValidTarget ? pTarget->GetDisplayName() : "Nobody";
		this.IsTargetLoading = pTarget != null && !pTarget->CanDraw();

		this.Target = pTarget;

		if (startIndex != this.TargetObjectIndex)
		{
			this.TargetChanged?.Invoke();
		}
	}
}

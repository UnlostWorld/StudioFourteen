namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Input;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfUtils.Extensions;

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

	public override Task Start()
	{
		this.Services.Input.MouseButton += this.OnMouseButton;
		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.Input.MouseButton -= this.OnMouseButton;
		return base.Stop();
	}

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

	private void OnMouseButton(MouseButton button, InputService.States state, Vector2 position)
	{
		if (button == MouseButton.Left && state == InputService.States.Released)
		{
			this.TargetPosition(position).Run();
		}
	}

	private async Task TargetPosition(Vector2 screenPosition)
	{
		await Threads.FrameworkThread();

		unsafe
		{
			HitInfo hitInfo = RayCast.Cast(screenPosition);

			if (hitInfo.ObjectTableIndex == -1)
				return;

			this.SetTarget(hitInfo.ObjectTableIndex);
		}
	}
}

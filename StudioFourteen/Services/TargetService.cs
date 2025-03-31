// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Plugin;
using StudioFourteen.Studio.Background;
using StudioFourteen.Utilities;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using System;

public partial class TargetService : ServiceBase
{
	private readonly InputActionListener nextTargetListener = new(InputAction.NextTarget);
	private readonly InputActionListener previousTargetListener = new(InputAction.PreviousTarget);

	[Notify] private string? characterName;
	[Notify] private bool hasValidTarget = false;
	[Notify] private bool isTargetLoading = false;
	[Notify] private int targetObjectIndex = -1;

	public TargetService()
	{
		this.nextTargetListener.Activate = this.OnNextTarget;
		this.previousTargetListener.Activate = this.OnPreviousTarget;

#if DEBUG
		if (DalamudServices.ObjectTable == null)
		{
			this.CharacterName = "Debug Character";
			this.HasValidTarget = true;
			this.IsTargetLoading = false;
			this.TargetObjectIndex = 1;
		}
#endif
	}

	public delegate void TargetChangedDelegate(int objectTableIndex);

	public event TargetChangedDelegate? TargetChanged;

	public int ObjectTableCount => DalamudServices.ObjectTable?.Length ?? 0;

	public override Task Start()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.Services.Panels.PanelsRestarted += this.OnPanelsRestarted;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);
		return base.Start();
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	public unsafe Character* GetCharacter(int objectTableIndex)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		IntPtr address = DalamudServices.ObjectTable?.GetObjectAddress(objectTableIndex) ?? IntPtr.Zero;
		if (address == IntPtr.Zero)
			return null;

		Character* pCharacter = (Character*)address;
		if (pCharacter == null)
			return null;

		if (pCharacter->ObjectKind == ObjectKind.Ornament
			|| pCharacter->ObjectKind == ObjectKind.Mount)
			return null;

		return pCharacter;
	}

	public unsafe void SetTarget(int objectTableIndex)
	{
		Threads.RunOnFrameworkThread(() =>
		{
			if (DalamudServices.ObjectTable == null)
				return;

			Character* target = this.GetCharacter(objectTableIndex);
			if (target == null)
				return;

			TargetSystem.Instance()->GPoseTarget = (GameObject*)target;
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
			return this.GetCharacter(0);
		}
	}

	public Task TargetPosition(Point screenPosition)
	{
		return this.TargetPosition(new Vector2((float)screenPosition.X, (float)screenPosition.Y));
	}

	public async Task TargetPosition(Vector2 screenPosition)
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

	public async Task<bool> MoveTarget(int fromObjectTableIndex)
	{
		await Threads.FrameworkThread();

		if (!this.Services.GroupPose.IsGroupPosing)
			return false;

		if (DalamudServices.ObjectTable == null)
			return false;

		int min = GroupPoseService.GPoseFirstCharacter;
		int max = min + GroupPoseService.GPoseCharacterCount;

		unsafe
		{
			Character* pTarget = null;
			for (int i = fromObjectTableIndex + 1; i < max; i++)
			{
				pTarget = this.GetCharacter(i);

				if (pTarget != null)
				{
					break;
				}
			}

			if (pTarget == null)
			{
				for (int i = fromObjectTableIndex - 1; i >= min; i--)
				{
					pTarget = this.GetCharacter(i);

					if (pTarget != null)
					{
						break;
					}
				}
			}

			if (pTarget == null)
				return false;

			this.SetTarget(pTarget->ObjectIndex);
			return true;
		}
	}

	protected unsafe void OnGameTick()
	{
		int startIndex = this.TargetObjectIndex;

		Character* pTarget = this.GetTarget();
		this.HasValidTarget = pTarget != null && pTarget->CanDraw();
		this.TargetObjectIndex = this.HasValidTarget ? pTarget->ObjectIndex : -1;
		this.CharacterName = this.HasValidTarget ? pTarget->GetDisplayName() : "Nobody";
		this.IsTargetLoading = pTarget != null && !pTarget->CanDraw();

		if (startIndex != this.TargetObjectIndex)
		{
			this.TargetChanged?.Invoke(this.targetObjectIndex);
		}
	}

	private void OnPanelsRestarted(PanelService self)
	{
		this.Services.Panels.GamePanels.SetIsOpen<TargetsPanel>(this.Services.GroupPose.IsGroupPosing, false);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.Services.Panels.GamePanels.SetIsOpen<TargetsPanel>(newState, false);

		if (newState)
		{
			this.nextTargetListener.Enable();
			this.previousTargetListener.Enable();
		}
		else
		{
			this.nextTargetListener.Disable();
			this.previousTargetListener.Disable();
		}
	}

	private unsafe void OnNextTarget()
	{
		Threads.RunOnFrameworkThread(() => this.AdvanceTarget(1));
	}

	private void OnPreviousTarget()
	{
		Threads.RunOnFrameworkThread(() => this.AdvanceTarget(-1));
	}

	private unsafe void AdvanceTarget(int count)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		int max = GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount;
		int newIndex = this.targetObjectIndex;
		Character* newTarget = null;

		int iterations = 200;
		while (newTarget == null && iterations > 0)
		{
			iterations--;
			newIndex += count;

			if (newIndex >= max)
			{
				newIndex = GroupPoseService.GPoseFirstCharacter;
			}

			if (newIndex < GroupPoseService.GPoseFirstCharacter)
			{
				newIndex = max;
			}

			newTarget = this.GetCharacter(newIndex);
		}

		if (newTarget != null)
		{
			this.SetTarget(newIndex);
		}
	}
}

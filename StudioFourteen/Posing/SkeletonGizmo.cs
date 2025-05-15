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

namespace StudioFourteen.Posing;

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Services;

public class SkeletonGizmo : SceneGroup
{
	public readonly int ObjectTableIndex;

	private readonly Dictionary<BoneId, BoneGizmo> boneGizmos = new();
	private bool isInitialized = false;

	public SkeletonGizmo(int objectTableIndex)
	{
		this.ObjectTableIndex = objectTableIndex;
	}

	public unsafe void Initialize()
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectTableIndex);
		if (pCharacter == null)
			return;

		CharacterBase* characterBase = pCharacter->GetCharacterBase();
		if (characterBase == null)
			return;

		ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

			byte poseIdx = 0;
			////byte poseCount = partialSkeleton->GetMaxPoses();
			////for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
			{
				hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
				if (pose == null)
					continue;

				int boneCount = pose->Skeleton->Bones.Length;
				for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
				{
					hkaBone bone = pose->Skeleton->Bones[boneIdx];
					string? boneName = bone.Name.String;

					if (boneName != null
						&& ServiceManager.Instance.Settings.Current.HideGenitals
						&& ServiceManager.Instance.Content.GenitalBones?.Contains(boneName) == true)
					{
						continue;
					}

					if (boneName == "n_root")
						continue;

					BoneId boneId = new(pCharacter->ObjectIndex, partialIdx, poseIdx, boneIdx);

					BoneGizmo gizmo = this.GetOrAddBoneGizmo(boneId);

					short parentIndex = pose->Skeleton->ParentIndices[boneIdx];
					if (parentIndex > 0)
					{
						BoneId parentBoneId = new(pCharacter->ObjectIndex, partialIdx, poseIdx, parentIndex);
						BoneGizmo parentGizmo = this.GetOrAddBoneGizmo(parentBoneId);
						parentGizmo.AddChild(boneId);
					}
				}
			}
		}

		this.isInitialized = this.boneGizmos.Count > 0;
	}

	public override void Dispose()
	{
		this.boneGizmos.Clear();
		base.Dispose();
	}

	protected unsafe override void OnDraw()
	{
		base.OnDraw();

		if (ServiceManager.Instance.GroupPose.IsGroupPosing
			&& this.ObjectTableIndex < GroupPoseService.GPoseFirstCharacter
			&& this.ObjectTableIndex > GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount)
			return;

		Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectTableIndex);
		if (pCharacter == null || pCharacter->DrawObject == null)
			return;

		if (!pCharacter->CanDraw())
			return;

		if (pCharacter->ObjectKind != ObjectKind.Pc
			&& pCharacter->ObjectKind != ObjectKind.BattleNpc
			&& pCharacter->ObjectKind != ObjectKind.EventNpc)
			return;

		if (!this.isInitialized)
			this.Initialize();

		float scale = pCharacter->GetCharacterScale();
		Transform modelTransform = StudioFourteen.Transform.FromTRS(
			pCharacter->DrawObject->Position,
			pCharacter->DrawObject->Rotation,
			pCharacter->DrawObject->Scale * scale);

		this.Transform = modelTransform;
	}

	private BoneGizmo GetOrAddBoneGizmo(BoneId boneId)
	{
		BoneGizmo? gizmo;
		if (!this.boneGizmos.TryGetValue(boneId, out gizmo))
		{
			gizmo = new(boneId);
			this.Add(gizmo);
			this.boneGizmos.Add(boneId, gizmo);
		}

		return gizmo;
	}
}

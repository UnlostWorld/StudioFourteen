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
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using SharpDX.Direct3D11;
using StudioFourteen.Interop.Structs;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Gizmos;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Services;

using Material = StudioFourteen.Rendering.Material;

public class SkeletonsGizmo : GizmoBase
{
	private readonly Dictionary<int, SkeletonGizmo> skeletonLookup = new();

	public override string Name => "Skeletons";

	public override void Enable()
	{
		base.Enable();

		this.Services.CharacterLifecycle.CharacterCreated += this.OnCharacterCreated;
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.Tick.Dispatch(TickService.Channels.GameTick, this.Initialize);
	}

	public override void Disable()
	{
		this.Services.CharacterLifecycle.CharacterCreated -= this.OnCharacterCreated;
		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;

		foreach((int objectTableIndex, SkeletonGizmo gizmo) in this.skeletonLookup)
		{
			this.Remove(gizmo);
			gizmo.Dispose();
		}

		this.skeletonLookup.Clear();

		base.Disable();
	}

	private unsafe void Initialize()
	{
		TickService.VerifyGameTickThread();

		Character*[] pCharacters = this.Services.CharacterLifecycle.GetAllCharacters();
		foreach(Character* pCharacter in pCharacters)
		{
			this.OnCharacterCreated(pCharacter->ObjectIndex);
		}
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		if (this.skeletonLookup.TryGetValue(objectTableIndex, out var skeletonGizmo))
		{
			skeletonGizmo.Visible = false;
			this.Remove(skeletonGizmo);
			skeletonGizmo.Dispose();
			this.skeletonLookup.Remove(objectTableIndex);
		}
	}

	private unsafe void OnCharacterCreated(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		if (this.skeletonLookup.ContainsKey(objectTableIndex))
			return;

		SkeletonGizmo gizmo = new(objectTableIndex);
		this.Add(gizmo);
		this.skeletonLookup.Add(objectTableIndex, gizmo);
	}
}

public class SkeletonGizmo : SceneGroup
{
	public readonly int ObjectTableIndex;

	private readonly Dictionary<BoneId, MeshRenderer> boneRenderers = new();
	private readonly Dictionary<(BoneId, BoneId), LineRenderer> connectionRenderers = new();
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

					BoneId boneId = new(pCharacter->ObjectIndex, partialIdx, poseIdx, boneIdx);

					if (!this.boneRenderers.ContainsKey(boneId))
					{
						////MeshRenderer renderer = new(Meshes.Bone, Material.Dot);
						////this.Add(renderer);
						////this.boneRenderers.Add(boneId, renderer);
					}

					short parentIndex = pose->Skeleton->ParentIndices[boneIdx];
					if (parentIndex != -1)
					{
						BoneId parentBoneId = new(pCharacter->ObjectIndex, partialIdx, poseIdx, parentIndex);
						if (!this.connectionRenderers.ContainsKey((boneId, parentBoneId)))
						{
							LineRenderer renderer = new(Material.Bone);
							renderer.Color = pCharacter->GetDisplayColor();

							this.Add(renderer);
							this.connectionRenderers.Add((boneId, parentBoneId), renderer);
						}
					}
				}
			}
		}

		this.isInitialized = this.boneRenderers.Count > 0;
	}

	public unsafe override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (ServiceManager.Instance.GroupPose.IsGroupPosing
			&& this.ObjectTableIndex < GroupPoseService.GPoseFirstCharacter
			&& this.ObjectTableIndex > GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount)
			return;

		Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectTableIndex);
		if (pCharacter == null)
			return;

		if (pCharacter->ObjectKind != ObjectKind.Pc
			&& pCharacter->ObjectKind != ObjectKind.BattleNpc
			&& pCharacter->ObjectKind != ObjectKind.EventNpc)
			return;

		if(!this.isInitialized)
			this.Initialize();

		Transform thisTransform = transform * this.Transform;

		////base.Draw(transform, device, deviceContext);

		foreach((BoneId id, MeshRenderer renderer) in this.boneRenderers)
		{
			if (!id.Resolve(out _, out Skeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
				continue;

			if (!pCharacter->CanDraw())
				continue;

			if (id.BoneIndex >= pPose->Skeleton->Bones.Length)
				continue;

			Transform modelSpaceTransform = *pPose->AccessBoneModelSpace(id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			float scale = pCharacter->GetCharacterScale();

			Transform modelTransform = Transform.FromTRS(
				pCharacter->DrawObject->Position,
				pCharacter->DrawObject->Rotation,
				pCharacter->DrawObject->Scale * scale);

			renderer.Transform = Transform.FromScale(0.25f);
			renderer.Transform *= modelSpaceTransform * modelTransform;

			renderer.Draw(thisTransform, device, deviceContext);
		}

		foreach(((BoneId id, BoneId parentId), LineRenderer renderer) in this.connectionRenderers)
		{
			if (!id.Resolve(out _, out Skeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
				continue;

			if (!pCharacter->CanDraw())
				continue;

			if (id.BoneIndex >= pPose->Skeleton->Bones.Length)
				continue;

			if (!parentId.Resolve(out Character* pParentCharacter, out Skeleton* pParentSkeleton, out PartialSkeleton* pParentPartialSkeleton, out hkaPose* pParentPose))
				continue;

			if (parentId.BoneIndex >= pParentPose->Skeleton->Bones.Length)
				continue;

			Transform modelSpaceTransform = *pPose->AccessBoneModelSpace(id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			Transform parentModelSpaceTransform = *pParentPose->AccessBoneModelSpace(parentId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

			CharacterBase* pBase = pCharacter->GetCharacterBase();
			float scale = pCharacter->GetCharacterScale();

			Transform modelTransform = Transform.FromTRS(
				pCharacter->DrawObject->Position,
				pCharacter->DrawObject->Rotation,
				pCharacter->DrawObject->Scale * scale);

			Transform boneTransform = modelSpaceTransform; // * modelTransform;
			Transform parentTransform = parentModelSpaceTransform; // * modelTransform;

			Vector3 bonePos = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());
			Vector3 parentPos = Vector3.Transform(Vector3.Zero, parentTransform.ToMatrix());
			Vector3 vector = bonePos - parentPos;

			renderer.From = parentPos;
			renderer.To = bonePos;

			renderer.Transform = modelTransform;

			renderer.Draw(thisTransform, device, deviceContext);
		}
	}

	public override void Dispose()
	{
		this.boneRenderers.Clear();
		this.connectionRenderers.Clear();
		base.Dispose();
	}
}
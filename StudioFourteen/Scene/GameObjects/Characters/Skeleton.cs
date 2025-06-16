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

namespace StudioFourteen.Scene.GameObjects.Characters;

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Posing;
using StudioFourteen.Services;

using XivDrawCharacter = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CharacterBase;
using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public class Skeleton : GameObject
{
	private readonly Dictionary<string, SkeletonBone> boneNameLookup = new();
	private readonly Dictionary<BoneId, BoneReference> boneReferenceLookup = new();
	private bool doGenerate = false;

	public Skeleton(int objectIndex)
		: base(objectIndex)
	{
		this.doGenerate = true;

		this.Services.Skeletons.AddSkeleton(this);
	}

	public List<SkeletonBone> Bones { get; init; } = new();

	public override void Dispose()
	{
		this.Services.Skeletons.RemoveSkeleton(this);
		base.Dispose();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		if (this.doGenerate)
		{
			this.doGenerate = false;
			this.GenerateBones();
		}
	}

	public void Export()
	{
	}

	public void Flip()
	{
	}

	public void SetToReferencePose()
	{
		/*foreach (BoneReference boneReference in this.boneReferences)
		{
			boneReference.SetToReference();
		}*/
	}

	public BoneReference? FindBoneReference(BoneId id)
	{
		this.boneReferenceLookup.TryGetValue(id, out BoneReference? reference);
		return reference;
	}

	public SkeletonBone? FindBone(string name)
	{
		this.boneNameLookup.TryGetValue(name, out SkeletonBone? bone);
		return bone;
	}

	public void OnUpdateBonePhysics(ref HashSet<nint> modifiedSkeletonPointers)
	{
		foreach (SkeletonBone bone in this.Bones)
		{
			bone.OnUpdateBonePhysics(ref modifiedSkeletonPointers);
		}
	}

	public void OnFinalizeSkeleton()
	{
		foreach (SkeletonBone bone in this.Bones)
		{
			bone.OnFinalizeSkeleton();
		}
	}

	protected override void OnLockTransformChanged(bool oldValue, bool newValue)
	{
		base.OnLockTransformChanged(oldValue, newValue);

		/*foreach (BoneReference reference in this.boneReferences)
		{
			reference.Locked = newValue;
		}*/
	}

	private unsafe void GenerateBones()
	{
		if (this.ObjectIndex != 0)
			return;

		TickService.VerifyGameTickThread();

		XivGameObject* pGameObject = this.GetXivGameObject();
		if (pGameObject == null)
			return;

		XivDrawCharacter* pCharacterBase = (XivDrawCharacter*)pGameObject->DrawObject;
		if (pCharacterBase == null)
			return;

		Dictionary<string, List<BoneReference>> boneLookup = new();

		ushort partialCount = pCharacterBase->Skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* pPartialSkeleton = &pCharacterBase->Skeleton->PartialSkeletons[partialIdx];

			byte poseCount = pPartialSkeleton->GetMaxPoses();
			for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
			{
				hkaPose* pPose = pPartialSkeleton->GetHavokPose(poseIdx);
				if (pPose == null)
					continue;

				int boneCount = pPose->Skeleton->Bones.Length;

				// Create bones
				for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
				{
					hkaBone bone = pPose->Skeleton->Bones[boneIdx];
					string? boneName = bone.Name.String;
					BoneId id = new(pGameObject->ObjectIndex, partialIdx, poseIdx, boneIdx);

					if (boneName == null)
						boneName = $"Unknown:{partialIdx}:{poseIdx}:{boneIdx}";

					if (!boneLookup.ContainsKey(boneName))
						boneLookup.Add(boneName, new());

					BoneId? parentId = null;
					int parentIndex = pPose->Skeleton->ParentIndices[boneIdx];
					if (parentIndex != -1)
						parentId = new(pGameObject->ObjectIndex, partialIdx, poseIdx, (short)parentIndex);

					BoneReference? reference = this.FindBoneReference(id);
					if (reference == null)
					{
						reference = new(this, id, parentId, boneName);
						this.boneReferenceLookup.Add(id, reference);
					}

					boneLookup[boneName].Add(reference);
				}
			}
		}

		lock (this.Bones)
		{
			foreach (SkeletonBone bone in this.Bones)
			{
				this.Services.Scene.RemoveObject(bone);
			}

			this.Bones.Clear();
			this.boneNameLookup.Clear();

			foreach ((string boneName, List<BoneReference> references) in boneLookup)
			{
				SkeletonBone bone = new(this, boneName, references);
				this.boneNameLookup.Add(boneName, bone);
				this.Services.Scene.AddObject(bone);
				this.Bones.Add(bone);
			}

			foreach (SkeletonBone bone in this.Bones)
			{
				foreach (BoneReference reference in bone.BoneReferences)
				{
					if (reference.ParentId == null)
						continue;

					BoneReference? parentReference = this.FindBoneReference(reference.ParentId.Value);
					if (parentReference == null)
						continue;

					if (this.boneNameLookup.TryGetValue(parentReference.BoneName, out SkeletonBone? newParent)
							&& newParent != null)
					{
						bone.SetParent(newParent);
					}
				}
			}
		}
	}
}
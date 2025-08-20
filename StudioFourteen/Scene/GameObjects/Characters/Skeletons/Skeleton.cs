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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using WpfUtils.Extensions;

using XivDrawCharacter = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.CharacterBase;
using XivGameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

public partial class Skeleton : GameObject
{
	private readonly Dictionary<string, SkeletonBone> boneNameLookup = new();
	private readonly Dictionary<BoneId, BoneReference> boneReferenceLookup = new();

	private bool hasGenerated = false;

	[Notify] private bool enablePosing = false;

	public Skeleton(int objectIndex)
		: base(objectIndex)
	{
		this.hasGenerated = false;
		this.Services.Skeletons.AddSkeleton(this);

		this.Gizmos.Add(new SkeletonGizmo(this));

		this.EnablePosing = this.ObjectIndex == 0 || this.ObjectIndex == GroupPoseService.GPoseFirstCharacter;
	}

	public List<SkeletonBone> Bones { get; init; } = new();

	public override void Dispose()
	{
		this.ClearBones();
		this.Services.Skeletons.RemoveSkeleton(this);
		base.Dispose();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		if (this.hasGenerated && !this.EnablePosing)
		{
			this.hasGenerated = false;
			this.ClearBones();
		}
		else if (!this.hasGenerated && this.EnablePosing)
		{
			this.hasGenerated = true;
			this.GenerateBones();
		}
	}

	public async Task SavePose(bool includeLegacyBones = true, HashSet<string>? includeBones = null, bool onlyEdits = false)
	{
		PoseFile file = await this.ExportPoseAsync(includeLegacyBones, includeBones, onlyEdits);
		await this.Services.Files.SaveFileAsync(file, $"{this.Name}'s Pose");
	}

	public async Task<PoseFile> ExportPoseAsync(bool includeLegacyBones = true, HashSet<string>? includeBones = null, bool onlyEdits = false)
	{
		await TickService.GameTick();

		PoseFile file = new();

		file.Bones = new();
		file.ReferenceRelativeBones = new();
		file.MainHand = null;
		file.OffHand = null;

		foreach ((BoneId id, BoneReference boneReference) in this.boneReferenceLookup)
		{
			if (boneReference.BoneName == null)
				continue;

			if (boneReference.BoneName == "n_root")
				continue;

			// We'll have duplicate bone names, since we support indexing all the duplicate
			// HkPose and PartialSkeleton bones, but we can fairly safely assume the first
			// bone will be the one we want (from the lowest HkPose and PartialSkeleton)
			if (file.Bones.ContainsKey(boneReference.BoneName))
				continue;

			if (file.ReferenceRelativeBones.ContainsKey(boneReference.BoneName))
				continue;

			if (onlyEdits && (boneReference.Transform == null || boneReference.IsBlendingOut))
				continue;

			while (boneReference.IsBlending)
				await Task.Delay(33);

			// Legacy bone format for backwards compatibility
			if (includeLegacyBones && boneReference.ModelSpaceTransform != null)
			{
				PoseFile.LegacyBoneTransform modelSpaceTransform = new();
				modelSpaceTransform.Position = boneReference.ModelSpaceTransform.Value.Translation;
				modelSpaceTransform.Rotation = boneReference.ModelSpaceTransform.Value.Rotation;
				modelSpaceTransform.Scale = boneReference.ModelSpaceTransform.Value.Scale;
				file.Bones.Add(boneReference.BoneName, modelSpaceTransform);
			}

			// New format bones
			if (boneReference.LocalSpaceTransform != null && boneReference.ReferenceTransform != null)
			{
				Transform? referenceRelative = boneReference.ReferenceRelativeTransform;
				if (referenceRelative == null)
					continue;

				if (includeBones == null || includeBones.Contains(boneReference.BoneName))
				{
					BoneTransform boneTransform = new();
					boneTransform.Locked = boneReference.Locked;

					// Null out components that are irrelevantly small
					if (!referenceRelative.Value.Translation.IsApproximately(Vector3.Zero, 0.001f))
						boneTransform.Translation = referenceRelative.Value.Translation;

					// If the rotation quat has no x,y, or z component, then ignore it, as 0,0,0,1 is identity, and
					// a W component without X,Y,Z components doesn't do anything afaik.
					if (!referenceRelative.Value.Rotation.X.IsApproximately(0, 0.001f)
						|| !referenceRelative.Value.Rotation.Y.IsApproximately(0, 0.001f)
						|| !referenceRelative.Value.Rotation.Z.IsApproximately(0, 0.001f))
					{
						boneTransform.Rotation = referenceRelative.Value.Rotation;
					}

					if (!referenceRelative.Value.Scale.IsApproximately(Vector3.One, 0.001f))
						boneTransform.Scale = referenceRelative.Value.Scale;

					file.ReferenceRelativeBones.Add(boneReference.BoneName, boneTransform);
				}
			}
		}

		return file;
	}

	public void Flip()
	{
		this.FlipAsync().Run();
	}

	public async Task FlipAsync()
	{
		await TickService.GameTick();

		foreach ((BoneId id, BoneReference boneReference) in this.boneReferenceLookup)
		{
			while (boneReference.IsBlending)
			{
				await Task.Delay(33);
			}
		}

		HashSet<BoneReference> processed = new();
		foreach ((BoneId id, BoneReference boneReference) in this.boneReferenceLookup)
		{
			if (boneReference.BoneName == null)
				continue;

			if (boneReference.BoneName == "n_root")
			{
				if (boneReference.Transform == null)
					boneReference.Transform = Transform.Identity;
				boneReference.Transform *= Transform.FromRotation(180, 0, 0);
			}
			else
			{
				if (processed.Contains(boneReference))
					continue;

				if (boneReference.Mirror != null)
				{
					processed.Add(boneReference);
					processed.Add(boneReference.Mirror);

					Transform? a = boneReference.LocalSpaceTransform;
					if (a == null)
						a = Transform.Identity;

					Transform? b = boneReference.Mirror.LocalSpaceTransform;
					if (b == null)
						b = Transform.Identity;

					a = FlipUtility.Flip(a.Value, MirrorModes.MirrorTRCopyS);
					b = FlipUtility.Flip(b.Value, MirrorModes.MirrorTRCopyS);
					boneReference.SetLocalSpaceTransform(b.Value);
					boneReference.Mirror.SetLocalSpaceTransform(a.Value);
				}
				else
				{
					processed.Add(boneReference);

					Transform? a = boneReference.LocalSpaceTransform;
					if (a == null)
						a = Transform.Identity;

					a = FlipUtility.Flip(a.Value, MirrorModes.MirrorTRCopyS);

					boneReference.SetLocalSpaceTransform(a.Value);
				}
			}
		}
	}

	public async Task ImportPose(PoseFile file, UpdateSource source, bool immediate = false)
	{
		await TickService.GameTick();

		bool useReferenceRelativeBones = file.ReferenceRelativeBones != null;

		foreach ((BoneId id, BoneReference boneReference) in this.boneReferenceLookup)
		{
			if (boneReference.BoneName == null)
				continue;

			if (boneReference.BoneName == "n_root")
				continue;

			if (useReferenceRelativeBones && file.ReferenceRelativeBones != null)
			{
				BoneTransform? val = null;
				if (file.ReferenceRelativeBones.TryGetValue(boneReference.BoneName, out val))
				{
					boneReference.SetReferenceRelativeTransform(val, !immediate);
					boneReference.Locked = val.Locked;
				}
				else
				{
					boneReference.Reset(immediate);
				}

				continue;
			}
			else
			{
				boneReference.Reset(true);

				// no face bones for legacy poses
				if (boneReference.BoneName.StartsWith("j_f_"))
					continue;

				PoseFile.LegacyBoneTransform? val = null;
				if (file.Bones?.TryGetValue(boneReference.BoneName, out val) != true)
				{
					string? legacyName = LegacyBoneNameConverter.GetLegacyName(boneReference.BoneName);
					if (legacyName != null)
					{
						file.Bones?.TryGetValue(legacyName, out val);
					}
				}

				if (val != null)
				{
					// TODO: Allow a way for the user to explicitly include translation & scale
					// but disable them by default (Anamnesis style)
					val.Position = null;
					val.Scale = null;

					boneReference.SetModelSpaceTransform(val.ToBoneTransform());
					boneReference.Locked = true;
					continue;
				}
			}
		}

		// Wait for any bone blends to complete.
		foreach ((BoneId id, BoneReference boneReference) in this.boneReferenceLookup)
		{
			while (boneReference.IsBlending)
			{
				await Task.Delay(10);
			}
		}
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

	public void ResetPose()
	{
		foreach (SkeletonBone bone in this.Bones)
		{
			bone.Reset();
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

	private void ClearBones()
	{
		foreach (SkeletonBone bone in this.Bones)
		{
			this.Services.Scene.RemoveObject(bone);
		}

		this.Bones.Clear();
		this.boneNameLookup.Clear();
	}

	private unsafe void GenerateBones()
	{
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
			this.ClearBones();

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
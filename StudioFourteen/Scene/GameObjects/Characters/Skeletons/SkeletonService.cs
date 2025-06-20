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

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;

using XivSkeleton = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

public partial class SkeletonService : ServiceBase
{
	private readonly List<Skeleton> skeletons = new();

	public static string? GetMirrorBoneName(string name)
	{
		if (name.EndsWith("_l"))
		{
			return name.Substring(0, name.Length - 2) + "_r";
		}
		else if (name.EndsWith("_r"))
		{
			return name.Substring(0, name.Length - 2) + "_l";
		}

		return null;
	}

	public unsafe override void Attach()
	{
		base.Attach();

		Hooks.UpdateBonePhysics.Enable(this.UpdateBonePhysicsDetour);
		Hooks.FinalizeSkeletons.Enable(this.FinalizeSkeletonDetour);
		Hooks.SetPosition.Enable(this.SetPosition);
	}

	public override void Detach()
	{
		base.Detach();

		Hooks.UpdateBonePhysics.Disable();
		Hooks.FinalizeSkeletons.Disable();
		Hooks.SetPosition.Disable();
	}

	public void AddSkeleton(Skeleton skeleton)
	{
		lock (this.skeletons)
		{
			this.skeletons.Add(skeleton);
		}
	}

	public void RemoveSkeleton(Skeleton skeleton)
	{
		lock (this.skeletons)
		{
			this.skeletons.Remove(skeleton);
		}
	}

	/*public void Flip(int objectTableIndex)
	{
		this.FlipAsync(objectTableIndex).Run();
	}

	public async Task FlipAsync(int objectTableIndex)
	{
		await TickService.GameTick();

		List<BoneReference> boneReferences = new();

		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
			if (pCharacter == null)
				return;

			CharacterBase* pCharacterBase = pCharacter->GetCharacterBase();
			if (pCharacterBase == null)
				return;

			ushort partialCount = pCharacterBase->Skeleton->PartialSkeletonCount;
			for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* pPartialSkeleton = &pCharacterBase->Skeleton->PartialSkeletons[partialIdx];

				////byte poseCount = partialSkeleton->GetMaxPoses();
				////for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
				byte poseIdx = 0;
				{
					hkaPose* pPose = pPartialSkeleton->GetHavokPose(poseIdx);
					if (pPose == null)
						continue;

					int boneCount = pPose->Skeleton->Bones.Length;

					// Create bone nodes
					for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
					{
						hkaBone bone = pPose->Skeleton->Bones[boneIdx];
						string boneName = bone.Name.String ?? "Bone";
						BoneId id = new(objectTableIndex, partialIdx, poseIdx, boneIdx);

						if (boneName == "n_root")
							continue;

						boneReferences.Add(this.GetOrCreateBoneReference(id, boneName));
					}
				}
			}
		}

		await Threads.NextFrame();

		foreach (BoneReference bone in boneReferences)
		{
			bone.Locked = true;

			Transform? transform = bone.ReferenceRelativeTransform;
			if (transform == null)
				continue;

			Transform flipped = FlipUtility.Flip(transform.Value);

			if (bone.Mirror != null)
			{
				bone.Mirror.SetReferenceRelativeTransform(flipped, true);
			}
			else
			{
				bone.SetReferenceRelativeTransform(flipped, true);
			}
		}
	}

	public async Task ExportPose(int objectTableIndex)
	{
		await TickService.GameTick();

		string name = $"#{objectTableIndex}";
		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
			name = pCharacter->GetDisplayName();
		}

		PoseFile file = new();
		await file.Save(objectTableIndex);
		this.Services.Files.SaveFile(file, $"{name}'s Pose");
	}*/

	private unsafe void SetPosition(GameObject* self, float x, float y, float z)
	{
		if (this.Services.GroupPose.IsGroupPoseLoaded)
			return;

		Hooks.SetPosition.Original(self, x, y, z);
	}

	private unsafe nint UpdateBonePhysicsDetour(nint a1)
	{
		nint result = Hooks.UpdateBonePhysics.Original(a1);

		try
		{
			if (this.Services.Studio.IsOpen)
			{
				this.UpdateBonePhysics();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error during skeleton update");
		}

		return result;
	}

	private void FinalizeSkeletonDetour(nint a1)
	{
		Hooks.FinalizeSkeletons.Original(a1);

		try
		{
			if (this.Services.Studio.IsOpen)
			{
				this.FinalizeSkeletons();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error during skeleton update");
		}
	}

	// This is a very hot path, be careful how much you do here.
	// All the main skeleton stuff like positions, IK and physics is done at this point.
	private unsafe void UpdateBonePhysics()
	{
		HashSet<nint> modifiedSkeletonPointers = new();

		lock (this.skeletons)
		{
			foreach (Skeleton skeleton in this.skeletons)
			{
				skeleton.OnUpdateBonePhysics(ref modifiedSkeletonPointers);
			}
		}

		// Update sub partials
		foreach (nint skeletonPtr in modifiedSkeletonPointers)
		{
			XivSkeleton* skeleton = (XivSkeleton*)skeletonPtr;
			if (skeleton == null)
				continue;

			ushort partialCount = skeleton->PartialSkeletonCount;
			if (partialCount <= 1)
				continue;

			for (int partialIdx = 1; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* partialSkeleton = &skeleton->PartialSkeletons[partialIdx];

				if (partialSkeleton->ConnectedBoneIndex >= 0 && partialSkeleton->ConnectedParentBoneIndex >= 0)
				{
					PartialSkeleton* parentPartial = &skeleton->PartialSkeletons[0];

					// assume pose 0
					hkaPose* pose = partialSkeleton->GetHavokPose(0);
					hkaPose* parentPose = parentPartial->GetHavokPose(0);

					hkQsTransformf* transform = pose->AccessBoneModelSpace(partialSkeleton->ConnectedBoneIndex, hkaPose.PropagateOrNot.Propagate);
					hkQsTransformf* parentTransform = parentPose->AccessBoneModelSpace(partialSkeleton->ConnectedParentBoneIndex, hkaPose.PropagateOrNot.DontPropagate);

					transform->Translation = parentTransform->Translation;
					transform->Rotation = parentTransform->Rotation;
					transform->Scale = parentTransform->Scale;
				}
			}
		}
	}

	private unsafe void FinalizeSkeletons()
	{
		lock (this.skeletons)
		{
			foreach (Skeleton skeleton in this.skeletons)
			{
				skeleton.OnFinalizeSkeleton();
			}
		}
	}
}
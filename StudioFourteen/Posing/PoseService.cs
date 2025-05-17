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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Interop;
using StudioFourteen.Interop.Structs;
using StudioFourteen.Selection;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using WpfUtils.Animation;
using WpfUtils.Extensions;

public enum MirrorModes
{
	/// <summary>
	/// Do not perform any mirroring action.
	/// </summary>
	None,

	/// <summary>
	/// (Mirror) Mirror the Translation and Rotation, and copy the scale.
	/// </summary>
	MirrorTRCopyS,

	/// <summary>
	/// (Copy) Mirror the bone Translation and copy the Rotation and Scale.
	/// </summary>
	MirrorTCopyRS,

	/// <summary>
	/// This object is receiving mirrors from its opposite.
	/// </summary>
	Receiving,
}

public partial class PoseService : ServiceBase
{
	private readonly List<BoneId> boneIds = new();
	private readonly Dictionary<BoneId, BoneReference> boneReferences = new();
	private readonly SkeletonsGizmo gizmo = new();

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

	public override Task Start()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChange;
		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;
		this.FlushBoneReferences();
		return base.Stop();
	}

	public unsafe override void Attach()
	{
		base.Attach();

		this.gizmo.Enable();

		Hooks.UpdateBonePhysics.Enable(this.UpdateBonePhysicsDetour);
		Hooks.FinalizeSkeletons.Enable(this.FinalizeSkeletonDetour);
		Hooks.SetPosition.Enable(this.SetPosition);
	}

	public override void Detach()
	{
		base.Detach();

		this.gizmo.Disable();

		Hooks.UpdateBonePhysics.Disable();
		Hooks.FinalizeSkeletons.Disable();
		Hooks.SetPosition.Disable();
	}

	public bool AreAllBoneReferencesLocked(int objectTableId)
	{
		int count = 0;
		lock (this.boneReferences)
		{
			foreach ((BoneId id, BoneReference reference) in this.boneReferences)
			{
				if (id.ObjectTableIndex != objectTableId)
					continue;

				if (!reference.Locked)
					return false;

				count++;
			}
		}

		return count > 0;
	}

	public async void SetAllBoneReferencesLocked(int objectTableIndex, bool locked)
	{
		await TickService.GameTick();

		List<BoneReference> references = await this.GetOrCreateBoneReferences(objectTableIndex);
		foreach (BoneReference reference in references)
		{
			reference.Locked = locked;
		}
	}

	public async Task SetToReferencePose(int objectTableIndex)
	{
		await TickService.GameTick();

		List<BoneReference> references = await this.GetOrCreateBoneReferences(objectTableIndex);
		await Threads.NextFrame();
		foreach (BoneReference reference in references)
		{
			reference.SetToReference();
		}
	}

	public async Task<List<BoneReference>> GetOrCreateBoneReferences(int objectTableIndex)
	{
		await TickService.GameTick();

		List<BoneReference> results = new();
		bool didCreate = false;
		results = this.GetOrCreateBoneReferencesUnsafe(objectTableIndex, ref didCreate);

		if (didCreate)
		{
			lock (this.boneReferences)
			{
				this.boneIds.Sort();
			}
		}

		if (didCreate)
			await Threads.NextFrame();

		return results;
	}

	public unsafe List<BoneReference> GetOrCreateBoneReferencesUnsafe(int objectTableIndex, ref bool didCreateNewReferences)
	{
		TickService.VerifyGameTickThread();
		List<BoneReference> results = new();
		didCreateNewReferences = false;

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
		if (pCharacter == null)
			return results;

		CharacterBase* pCharacterBase = pCharacter->GetCharacterBase();
		if (pCharacterBase == null)
			return results;

		lock (this.boneReferences)
		{
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

					// Create bone nodes
					for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
					{
						hkaBone bone = pPose->Skeleton->Bones[boneIdx];
						string? boneName = bone.Name.String;
						BoneId id = new(objectTableIndex, partialIdx, poseIdx, boneIdx);

						if (this.boneReferences.TryGetValue(id, out BoneReference? reference))
						{
							results.Add(reference);
						}
						else
						{
							didCreateNewReferences = true;
							reference = new(id, boneName);
							this.boneIds.Add(id);
							this.boneReferences.Add(id, reference);
							results.Add(reference);
						}

						results.Add(this.GetOrCreateBoneReference(id, boneName));
					}
				}
			}
		}

		return results;
	}

	public BoneReference GetOrCreateBoneReference(BoneId id, string? name = null)
	{
		lock (this.boneReferences)
		{
			if (this.boneReferences.TryGetValue(id, out BoneReference? reference))
			{
				if (reference.Name == null && name != null)
					reference.Name = name;

				return reference;
			}

			reference = new(id, name);

			this.boneIds.Add(id);
			this.boneIds.Sort();
			this.boneReferences.Add(id, reference);

			return reference;
		}
	}

	public unsafe HashSet<string> GetAllBoneNames(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
		if (pCharacter == null)
			return new();

		return this.GetAllBoneNames(pCharacter);
	}

	public unsafe HashSet<string> GetAllBoneNames(Character* pCharacter)
	{
		TickService.VerifyGameTickThread();

		HashSet<string> results = new();

		CharacterBase* characterBase = pCharacter->GetCharacterBase();
		if (characterBase == null)
			return results;

		ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

			////byte poseCount = partialSkeleton->GetMaxPoses();
			////for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
			byte poseIdx = 0;
			{
				hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
				if (pose == null)
					continue;

				int boneCount = pose->Skeleton->Bones.Length;
				for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
				{
					hkaBone bone = pose->Skeleton->Bones[boneIdx];
					string? boneName = bone.Name.String;

					if (boneName == null || boneName == "n_root")
						continue;

					results.Add(boneName);
				}
			}
		}

		return results;
	}

	public unsafe BoneSelection? FindBone(int objectTableIndex, string name)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
		if (pCharacter == null)
			return null;

		return this.FindBone(pCharacter, name);
	}

	public unsafe BoneSelection? FindBone(Character* character, string name)
	{
		TickService.VerifyGameTickThread();

		CharacterBase* characterBase = character->GetCharacterBase();
		if (characterBase == null)
			return null;

		Dictionary<BoneId, List<BoneId>> bones = new();

		ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

			////byte poseCount = partialSkeleton->GetMaxPoses();
			////for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
			byte poseIdx = 0;
			{
				hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
				if (pose == null)
					continue;

				int boneCount = pose->Skeleton->Bones.Length;
				for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
				{
					hkaBone bone = pose->Skeleton->Bones[boneIdx];
					string? boneName = bone.Name.String;

					if (boneName == name)
					{
						BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);
						bones.Add(boneId, new());

						BoneId? parent = boneId;
						while(parent != null)
						{
							short parentIndex = pose->Skeleton->ParentIndices[parent.Value.BoneIndex];
							if (parentIndex != -1)
							{
								BoneId parentBoneId = new(character->ObjectIndex, partialIdx, poseIdx, parentIndex);
								bones[boneId].Add(parentBoneId);
								parent = parentBoneId;
							}
							else
							{
								parent = null;
							}
						}
					}
				}
			}
		}

		if (bones.Count <= 0)
			return null;

		return new BoneSelection(bones, name);
	}

	public void FlushBoneReferences()
	{
		lock (this.boneReferences)
		{
			foreach ((BoneId id, BoneReference reference) in this.boneReferences)
				reference.Dispose();

			this.boneReferences.Clear();
			this.boneIds.Clear();
		}
	}

	public unsafe void FlushBoneReferences(Character* character)
	{
		this.FlushBoneReferences(character->ObjectIndex);
	}

	public bool HasBoneReferences(int objectTableIndex)
	{
		lock (this.boneReferences)
		{
			foreach ((BoneId id, BoneReference reference) in this.boneReferences)
			{
				if (id.ObjectTableIndex == objectTableIndex)
				{
					return true;
				}
			}
		}

		return false;
	}

	public void FlushBoneReferences(int objectTableIndex)
	{
		HashSet<BoneId> toRemove = new();

		lock (this.boneIds)
		{
			lock (this.boneReferences)
			{
				foreach ((BoneId id, BoneReference reference) in this.boneReferences)
				{
					if (id.ObjectTableIndex == objectTableIndex)
					{
						toRemove.Add(id);
						reference.Dispose();
					}
				}

				foreach (BoneId id in toRemove)
				{
					this.boneReferences.Remove(id);
					this.boneIds.Remove(id);
				}
			}
		}

		// if we are flushing a bone we have selected, clear the selection
		if (this.Services.Selection.Current is BoneSelection boneSelection)
		{
			foreach (BoneId usedId in boneSelection.BonePaths.Keys)
			{
				if (toRemove.Contains(usedId))
				{
					this.Services.Selection.Current = null;
					break;
				}
			}
		}
	}

	public void Flip(int objectTableIndex)
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
		string name = $"#{objectTableIndex}";
		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTableIndex);
			name = pCharacter->GetDisplayName();
		}

		/*// Mouth
		HashSet<string> mouthBones = new()
		{
			"j_f_umlip_01_l",
			"j_f_umlip_02_l",
			"j_f_ulip_01_l",
			"j_f_ulip_02_l",
			"j_f_uslip_l",
			"j_f_dmlip_01_l",
			"j_f_dmlip_02_l",
			"j_f_dlip_01_l",
			"j_f_dlip_02_l",
			"j_f_dslip_l",
			"j_f_dmemoto_l",
			"j_f_shoho_l",

			"j_f_umlip_01_r",
			"j_f_umlip_02_r",
			"j_f_ulip_01_r",
			"j_f_ulip_02_r",
			"j_f_uslip_r",
			"j_f_dmlip_01_r",
			"j_f_dmlip_02_r",
			"j_f_dlip_01_r",
			"j_f_dlip_02_r",
			"j_f_dslip_r",
			"j_f_dmemoto_r",
			"j_f_shoho_r",

			"j_f_ago",

			"j_f_bero_01",
			"j_f_bero_02",
			"j_f_bero_03",
		};

		// Eyes
		HashSet<string> eyeBones = new()
		{
			"j_f_mabdn_01_l",
			"j_f_mabdn_02out_l",
			"j_f_mabdn_03in_l",
			"j_f_mabup_01_l",
			"j_f_mabup_02out_l",
			"j_f_mabup_03in_l",
			"j_f_hoho_l",
			"j_f_dhoho_l",
			"j_f_mayu_l",
			"j_f_mmayu_l",
			"j_f_miken_01_l",
			"j_f_miken_02_l",
			"j_f_dmiken_l",
			"j_f_mab_l",
		};*/

		PoseFile file = new();
		await file.Save(objectTableIndex);
		this.Services.Files.SaveFile(file, $"{name}'s Pose");
	}

	private unsafe void SetPosition(GameObject* self, float x, float y, float z)
	{
		if (this.Services.GroupPose.IsGroupPoseLoaded)
			return;

		Hooks.SetPosition.Original(self, x, y, z);
	}

	private async Task MoveTarget(Vector3 toPosition)
	{
		await TickService.GameTick();

		int objectTargetIndex = this.Services.Target.TargetObjectIndex;
		EasingFunctionBase ease = new SineEase();

		Vector3 fromPosition;
		unsafe
		{
			Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTargetIndex);
			fromPosition = pCharacter->DrawObject->Position;
		}

		// Extremely simple and wonky lerp.
		float duration = 500;
		float time = 0;
		while (time < duration)
		{
			await Task.Delay(33);
			time += 33;
			float p = Math.Clamp(time / duration, 0, 1);
			p = ease.Ease(p, EasingFunctionBase.EasingModes.EaseInOut);

			Vector3 newPosition = Vector3.Lerp(fromPosition, toPosition, p);

			await TickService.GameTick();

			unsafe
			{
				Character* pCharacter = this.Services.GameObjects.Get<Character>(objectTargetIndex);
				pCharacter->DrawObject->Position = newPosition;
			}
		}
	}

	private unsafe nint UpdateBonePhysicsDetour(nint a1)
	{
		nint result = Hooks.UpdateBonePhysics.Original(a1);

		try
		{
			if (this.Services.Studio.IsOpen)
			{
				this.UpdateSkeletons();
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
	private unsafe void UpdateSkeletons()
	{
		List<BoneId> boneIds;
		lock (this.boneIds)
		{
			boneIds = new(this.boneIds);
		}

		lock (this.boneReferences)
		{
			HashSet<nint> modifiedSkeletonPointers = new();

			foreach (BoneId boneId in boneIds)
			{
				BoneReference reference = this.boneReferences[boneId];
				if (!reference.IsValid)
					continue;

				Skeleton* skeleton = reference.Tick();
				if (skeleton == null)
					continue;

				modifiedSkeletonPointers.Add((nint)skeleton);
			}

			// Update sub partials
			foreach (nint skeletonPtr in modifiedSkeletonPointers)
			{
				Skeleton* skeleton = (Skeleton*)skeletonPtr;
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
	}

	private unsafe void FinalizeSkeletons()
	{
		lock (this.boneIds)
		{
			List<BoneId> boneIds = new(this.boneIds);

			lock (this.boneReferences)
			{
				foreach (BoneId boneId in boneIds)
				{
					if (!this.boneReferences.TryGetValue(boneId, out BoneReference? reference))
						continue;

					if (reference == null || !reference.IsValid)
						continue;

					reference.FinalizeBones();
				}
			}
		}
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		this.FlushBoneReferences(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		this.FlushBoneReferences();
	}
}
// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace ScreenshotStudio.Posing;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

public enum PoseEditModes
{
	Translation,
	Rotation,
	Scale,
}

public class PoseService : ServiceBase
{
	private readonly List<BoneId> boneIds = new();
	private readonly Dictionary<BoneId, BoneReference> boneReferences = new();

	private SelectionBase? selection;

	private Hook<UpdateBonePhysicsDelegate>? updateBonePhysicsHook;
	private PoseEditModes editMode = PoseEditModes.Rotation;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);
	private delegate nint UpdateBonePhysicsDelegate(nint a1);
	private delegate void FinalizeSkeletonsDelegate(nint a1);

	public event SelectionChangedDelegate? SelectionChanged;

	public SelectionBase? Selection
	{
		get => this.selection;
		set
		{
			this.selection?.Deactivate();

			this.selection = value;

			if (this.selection != null)
			{
				this.selection.Activate();

				// TODO: if they've changed the default?
				this.EditMode = this.selection.DefaultEditMode;
			}

			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();
		}
	}

	public PoseEditModes EditMode
	{
		get => this.editMode;
		set
		{
			this.editMode = value;
			this.RaisePropertyChanged();
		}
	}

	public override Task Start()
	{
		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChange;

		return base.Start();
	}

	public override Task Stop()
	{
		this.updateBonePhysicsHook?.Dispose();

		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;

		this.FlushBoneReferences();

		return base.Stop();
	}

	public bool AreAllBoneReferencesLocked(int objectTableId)
	{
		int count = 0;
		foreach((BoneId id, BoneReference reference) in this.boneReferences)
		{
			if (id.ObjectTableIndex != objectTableId)
				continue;

			if (!reference.Locked)
				return false;

			count++;
		}

		return count > 0;
	}

	public async void SetAllBoneReferencesLocked(int objectTableIndex, bool locked)
	{
		await Threads.FrameworkThread();

		List<BoneReference> references = this.GetOrCreateBoneReferences(objectTableIndex);
		foreach (BoneReference reference in references)
		{
			reference.Locked = locked;
		}
	}

	public async Task SetToReferencePose(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		List<BoneReference> references = this.GetOrCreateBoneReferences(objectTableIndex);
		await Threads.NextFrame();
		foreach(BoneReference reference in references)
		{
			reference.SetToReference();
		}
	}

	public List<BoneReference> GetOrCreateBoneReferences(int objectTableIndex)
	{
		List<BoneReference> results = new();

		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return results;

		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			if (pCharacter == null)
				return results;

			CharacterBase* pCharacterBase = pCharacter->GetCharacterBase();
			if (pCharacterBase == null)
				return results;

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
						string boneName = bone.Name.String ?? "Bone";
						BoneId id = new(objectTableIndex, partialIdx, poseIdx, boneIdx);

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

	public unsafe BoneSelection? FindBone(Character* character, string name)
	{
		Threads.VerifyFrameworkThread();

		CharacterBase* characterBase = character->GetCharacterBase();
		if (characterBase == null)
			return null;

		List<BoneId> bones = new();
		List<BoneId> parents = new();
		ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
		for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
		{
			PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

			byte poseCount = partialSkeleton->GetMaxPoses();
			for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
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
						bones.Add(new(character->ObjectIndex, partialIdx, poseIdx, boneIdx));

						short parentIndex = pose->Skeleton->ParentIndices[boneIdx];
						if (parentIndex != -1)
						{
							parents.Add(new(character->ObjectIndex, partialIdx, poseIdx, parentIndex));
						}
					}
				}
			}
		}

		if (bones.Count <= 0)
			return null;

		return new BoneSelection(bones, parents, name);
	}

	public void FlushBoneReferences()
	{
		lock (this.boneReferences)
		{
			foreach ((BoneId id, BoneReference reference) in this.boneReferences)
				reference.Clear();

			this.boneReferences.Clear();
			this.boneIds.Clear();
		}
	}

	public unsafe void FlushBoneReferences(Character* character)
	{
		this.FlushBoneReferences(character->ObjectIndex);
	}

	public void FlushBoneReferences(int objectTableIndex)
	{
		HashSet<BoneId> toRemove = new();

		lock (this.boneReferences)
		{
			foreach ((BoneId id, BoneReference reference) in this.boneReferences)
			{
				if (id.ObjectTableIndex == objectTableIndex)
				{
					toRemove.Add(id);
					reference.Clear();
				}
			}

			foreach (BoneId id in toRemove)
			{
				this.boneReferences.Remove(id);
				this.boneIds.Remove(id);
			}
		}

		// if we are flushing a bone we have selected, clear the selection
		if (this.selection is BoneSelection boneSelection)
		{
			foreach(BoneId usedId in boneSelection.BoneIds)
			{
				if (toRemove.Contains(usedId))
				{
					this.Selection = null;
					break;
				}
			}
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
		this.Selection?.OnFrameworkUpdate(framework);
	}

	private unsafe nint UpdateBonePhysicsDetour(nint a1)
	{
		if (this.updateBonePhysicsHook == null)
			return 0;

		nint result = this.updateBonePhysicsHook.Original(a1);

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

	// This is a very hot path, be careful how much you do here.
	// All the main skeleton stuff like positions, IK and physics is done at this point.
	private unsafe void UpdateSkeletons()
	{
		lock (this.boneReferences)
		{
			HashSet<nint> modifiedSkeletonPointers = new();

			foreach(BoneId boneId in this.boneIds)
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

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		this.FlushBoneReferences(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		this.FlushBoneReferences();
	}
}
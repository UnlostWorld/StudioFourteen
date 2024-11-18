// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace StudioFourteen.Posing;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FontAwesome.Sharp;
using StudioFourteen.Context;
using StudioFourteen.Files;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Animation;
using WpfUtils.Extensions;

public enum PoseEditModes
{
	Translation,
	Rotation,
	Scale,
}

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

public class PoseService : ServiceBase, WorldContextMenu.IProvider
{
	private readonly List<BoneId> boneIds = new();
	private readonly Dictionary<BoneId, BoneReference> boneReferences = new();

	private SelectionBase? selection;

	private Hook<UpdateBonePhysicsDelegate>? updateBonePhysicsHook;
	private Hook<FinalizeSkeletonsDelegate>? finalizeSkeletonsHook;
	private PoseEditModes editMode = PoseEditModes.Rotation;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);
	public delegate void EditModeChangedDelegate(PoseEditModes newMode);
	private delegate nint UpdateBonePhysicsDelegate(nint a1);
	private delegate void FinalizeSkeletonsDelegate(nint a1);

	public event SelectionChangedDelegate? SelectionChanged;
	public event EditModeChangedDelegate? EditModeChanged;

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
				if (this.selection is TransformSelectionBase transformSelection)
				{
					this.EditMode = transformSelection.DefaultEditMode;
				}
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
			this.EditModeChanged?.Invoke(value);
		}
	}

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
		WorldContextMenu.AddProvider(this);
		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;
		this.FlushBoneReferences();
		WorldContextMenu.RemoveProvider(this);
		return base.Stop();
	}

	public override void Attach()
	{
		base.Attach();

		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		// JMP in Framework.TaskRenderGraphicsRender
		this.finalizeSkeletonsHook = InteropService.HookFromSignature<FinalizeSkeletonsDelegate>("40 53 57 41 55 48 83 EC ?? 65 48 8B 04 25 58", this.FinalizeSkeletonDetour);
		this.finalizeSkeletonsHook?.Enable();
	}

	public override void Detach()
	{
		base.Detach();

		this.updateBonePhysicsHook?.Dispose();
		this.finalizeSkeletonsHook?.Dispose();
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

	public unsafe BoneSelection? FindBone(int objectTableIndex, string name)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
		return this.FindBone(character, name);
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

	public void Flip(int objectTableIndex)
	{
		this.FlipAsync(objectTableIndex).Run();
	}

	public async Task FlipAsync(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		List<BoneReference> boneReferences = new();

		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			if (pCharacter == null)
				return;

			CharacterBase* pCharacterBase = pCharacter->GetCharacterBase();
			if (pCharacterBase == null)
				return;

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

						if (boneName == "n_root")
							continue;

						boneReferences.Add(this.GetOrCreateBoneReference(id, boneName));
					}
				}
			}
		}

		await Threads.NextFrame();

		foreach(BoneReference bone in boneReferences)
		{
			bone.Locked = true;

			Transform? transform = bone.ReferenceRelativeTransform;
			if (transform == null)
				continue;

			Transform flipped = FlipUtility.Flip(transform.Value);

			if (bone.Mirror != null)
			{
				bone.Mirror.SetReferenceRelativeTransform(flipped);
			}
			else
			{
				bone.SetReferenceRelativeTransform(flipped);
			}
		}
	}

	public async Task ExportPose(int objectTableIndex)
	{
		if (DalamudServices.ObjectTable == null)
			return;

		string name = $"#{objectTableIndex}";
		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			name = pCharacter->GetDisplayName();
		}

		PoseFile file = new();
		await file.Save(objectTableIndex);
		this.Services.Files.SaveFile(file, $"{name}'s Pose");
	}

	public Task GetMenu(WorldContextMenu menu)
	{
		if (menu.IsObject)
		{
			bool hasReference = this.HasBoneReferences(menu.ObjectTableIndex);
			menu.Add(IconChar.RotateLeft, "Restore Pose", hasReference, (h) =>
			{
				this.FlushBoneReferences(h.ObjectTableIndex);
				return Task.CompletedTask;
			});

			menu.Add(IconChar.Save, "Export Pose", true, (h) => this.ExportPose(h.ObjectTableIndex));
		}
		else
		{
			menu.Add(IconChar.ArrowLeft, $"Move {this.Services.Target.CharacterName} Here", true, (h) => this.MoveTarget(h.Position));
		}

		return Task.CompletedTask;
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
		this.Selection?.OnFrameworkUpdate(framework);
	}

	private async Task MoveTarget(Vector3 toPosition)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		int objectTargetIndex = this.Services.Target.TargetObjectIndex;
		EasingFunctionBase ease = new SineEase();

		Vector3 fromPosition;
		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTargetIndex);
			fromPosition = pCharacter->DrawObject->Position;
		}

		// Extremely simple and wonky lerp.
		float duration = 500;
		float time = 0;
		while(time < duration)
		{
			await Task.Delay(33);
			time += 33;
			float p = Math.Clamp(time / duration, 0, 1);
			p = ease.Ease(p, EasingFunctionBase.EasingModes.EaseInOut);

			Vector3 newPosition = Vector3.Lerp(fromPosition, toPosition, p);

			await Threads.FrameworkThread();

			unsafe
			{
				Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTargetIndex);
				pCharacter->DrawObject->Position = newPosition;
			}
		}
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

	private void FinalizeSkeletonDetour(nint a1)
	{
		if (this.finalizeSkeletonsHook == null)
			return;

		this.finalizeSkeletonsHook.Original(a1);

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
		lock(this.boneIds)
		{
			boneIds = new(this.boneIds);
		}

		lock (this.boneReferences)
		{
			HashSet<nint> modifiedSkeletonPointers = new();

			foreach(BoneId boneId in boneIds)
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
		List<BoneId> boneIds;
		lock (this.boneIds)
		{
			boneIds = new(this.boneIds);
		}

		lock (this.boneReferences)
		{
			foreach (BoneId boneId in boneIds)
			{
				BoneReference reference = this.boneReferences[boneId];
				if (!reference.IsValid)
					continue;

				reference.FinalizeBones();
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
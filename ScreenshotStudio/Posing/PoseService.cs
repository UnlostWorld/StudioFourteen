// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.Matrix;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public class PoseService : ServiceBase
{
	private readonly Dictionary<BoneId, BoneReference> boneReferences = new();

	private SelectionBase? selection;

	private Hook<UpdateBonePhysicsDelegate>? updateBonePhysicsHook;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);
	private delegate nint UpdateBonePhysicsDelegate(nint a1);
	private delegate void FinalizeSkeletonsDelegate(nint a1);

	public event SelectionChangedDelegate? SelectionChanged;

	public SelectionBase? Selection
	{
		get => this.selection;
		set
		{
			if (this.selection != null)
				this.selection.Deactivate();

			this.selection = value;

			if (this.selection != null)
				this.selection.Activate();

			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();
		}
	}

	public override Task Start()
	{
		this.updateBonePhysicsHook = InteropService.HookFromSignature<UpdateBonePhysicsDelegate>("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 41 56 41 57 48 83 EC ?? 48 8B 79 ?? 45 33 FF", this.UpdateBonePhysicsDetour);
		this.updateBonePhysicsHook?.Enable();

		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChange += this.OnGroupPoseStateChange;

		return base.Start();
	}

	public override Task Shutdown()
	{
		this.updateBonePhysicsHook?.Dispose();

		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;
		this.Services.GroupPose.StateChange -= this.OnGroupPoseStateChange;

		this.FlushBoneReferences();

		return base.Shutdown();
	}

	public BoneReference GetOrCreateBoneReference(BoneId id, string? name = null)
	{
		BoneReference? reference = null;
		if (this.boneReferences.TryGetValue(id, out reference))
			return reference;

		reference = new(name, id);
		this.boneReferences.Add(id, reference);

		this.Log.Information($"Creating bone reference {id}");

		return reference;
	}

	public unsafe BoneSelection? FindBone(ref Character* character, string name)
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
		foreach ((BoneId id, BoneReference reference) in this.boneReferences)
			reference.Dispose();

		this.boneReferences.Clear();
	}

	public void FlushBoneReferences(uint objectTableIndex)
	{
		HashSet<BoneId> toRemove = new();
		foreach((BoneId id, BoneReference reference) in this.boneReferences)
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

	// This is a very hot path, be careful how much you do here.
	// All the main skeleton stuff like positions, IK and physics is done at this point.
	private void UpdateSkeletons()
	{
		foreach ((BoneId id, BoneReference reference) in this.boneReferences)
		{
			if (!reference.IsValid)
				continue;

			reference.ApplyTransform();
		}
	}

	private void OnCharacterDestroyed(uint objectTableIndex)
	{
		this.FlushBoneReferences(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		this.FlushBoneReferences();
	}
}
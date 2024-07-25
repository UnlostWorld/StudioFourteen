namespace ScreenshotStudio.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;

public class BoneReference : IDisposable
{
	public readonly string? Name;
	public readonly BoneId Id;

	public hkQsTransformf LastTransform;
	public hkQsTransformf CurrentTransform;
	public bool LockTransform;
	public hkQsTransformf LastLocalransform;

	public BoneReference? Parent;
	public bool IsValid = true;

	public BoneReference(string? name, BoneId id)
	{
		this.Name = name;
		this.Id = id;

		this.CurrentTransform = default;
		this.CurrentTransform.Rotation = HkQuaternionExtensions.Identity;
	}

	public unsafe Skeleton* Skeleton
	{
		get
		{
			if (DalamudServices.ObjectTable == null)
				return null;

			Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(this.Id.ObjectTableIndex);
			if (character == null)
				return null;

			CharacterBase* characterBase = character->GetCharacterBase();
			if (characterBase == null)
				return null;

			return characterBase->Skeleton;
		}
	}

	public unsafe PartialSkeleton* PartialSkeleton
	{
		get
		{
			if (this.Skeleton == null)
				return null;

			return &this.Skeleton->PartialSkeletons[this.Id.PartialSkeletonIndex];
		}
	}

	public unsafe hkaPose* Pose
	{
		get
		{
			if (this.PartialSkeleton == null)
				return null;

			return this.PartialSkeleton->GetHavokPose(this.Id.PoseIndex);
		}
	}

	public unsafe hkaBone? Bone
	{
		get
		{
			if (this.Pose == null)
				return null;

			return this.Pose->Skeleton->Bones[this.Id.BoneIndex];
		}
	}

	public unsafe void ApplyTransform()
	{
		Threads.VerifyFrameworkThread();

		if (!this.LockTransform)
		{
			// Get a new copy of the live transforms
			this.LastTransform = *this.Pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			this.LastLocalransform = *this.Pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			// Modify the live transform
			hkQsTransformf* transform = this.Pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			transform->Translation.Add(this.CurrentTransform.Translation);
			transform->Rotation.Multiply(this.CurrentTransform.Rotation);
			transform->Scale.Add(this.CurrentTransform.Scale);
		}
		else
		{
			hkQsTransformf newTransform = this.LastLocalransform;

			newTransform.Translation.Add(this.CurrentTransform.Translation);
			newTransform.Rotation.Multiply(this.CurrentTransform.Rotation);
			newTransform.Scale.Add(this.CurrentTransform.Scale);

			hkQsTransformf* transform = this.Pose->AccessBoneLocalSpace(this.Id.BoneIndex);
			transform->Translation.Set(newTransform.Translation);
			transform->Rotation.Set(newTransform.Rotation);
			transform->Scale.Set(newTransform.Scale);
		}
	}

	public void Dispose()
	{
		this.Parent = null;
		this.IsValid = false;
	}
}
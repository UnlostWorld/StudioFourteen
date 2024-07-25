namespace ScreenshotStudio.Services;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using System;

public class BoneReference(string? name, BoneId id) : IDisposable
{
	public readonly string? Name = name;
	public readonly BoneId Id = id;

	public BoneReference? Parent { get; set; }

	public bool IsValid { get; set; } = true;

	public hkQsTransformf LastTransform { get; set; }

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

	public unsafe void UpdateCachedTransform()
	{
		Threads.VerifyFrameworkThread();

		this.LastTransform = *this.Pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
	}

	public void Dispose()
	{
		this.Parent = null;
		this.IsValid = false;
	}
}

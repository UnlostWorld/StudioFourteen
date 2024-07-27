namespace ScreenshotStudio.Posing;

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
using System.Numerics;

public class BoneReference : IDisposable
{
	public readonly string? Name;
	public readonly BoneId Id;

	public Vector3 LastCharacterTranslation;
	public Quaternion LastCharacterRotation;
	public Vector3 LastCharacterScale;

	public hkQsTransformf LastTransform;
	public hkQsTransformf CurrentTransform;
	public bool LockTransform;
	public hkQsTransformf LastLocalTransform;

	public BoneReference? Parent;
	public bool IsValid = true;

	public BoneReference(string? name, BoneId id)
	{
		this.Name = name;
		this.Id = id;

		this.CurrentTransform = default;
		this.CurrentTransform.Rotation = HkQuaternionExtensions.Identity;
	}

	public unsafe Skeleton* ApplyTransform()
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(this.Id.ObjectTableIndex);
		if (character == null)
			return null;

		if (!character->CanDraw())
			return null;

		this.LastCharacterTranslation = character->DrawObject->Position;
		this.LastCharacterRotation = character->DrawObject->Rotation;
		this.LastCharacterScale = character->DrawObject->Scale;

		CharacterBase* characterBase = character->GetCharacterBase();
		if (characterBase == null)
			return null;

		Skeleton* skeleton = characterBase->Skeleton;
		if (skeleton == null)
			return null;

		PartialSkeleton* partialSkeleton = &skeleton->PartialSkeletons[this.Id.PartialSkeletonIndex];

		if (partialSkeleton == null)
			return null;

		hkaPose* pose = partialSkeleton->GetHavokPose(this.Id.PoseIndex);

		// Sanity check bone name, useful if the skeleton has changed during posing.
		if (this.Id.BoneName != null)
		{
			hkaBone bone = pose->Skeleton->Bones[this.Id.BoneIndex];
			if (bone.Name.String != this.Id.BoneName)
			{
				return null;
			}
		}

		if (!this.LockTransform)
		{
			// Get a new copy of the live transforms
			this.LastTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			this.LastLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			// Modify the live transform
			hkQsTransformf* transform = pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			transform->Translation.Add(this.CurrentTransform.Translation);
			transform->Rotation.Multiply(this.CurrentTransform.Rotation);
			transform->Scale.Add(this.CurrentTransform.Scale);
		}
		else
		{
			hkQsTransformf newTransform = this.LastLocalTransform;

			newTransform.Translation.Add(this.CurrentTransform.Translation);
			newTransform.Rotation.Multiply(this.CurrentTransform.Rotation);
			newTransform.Scale.Add(this.CurrentTransform.Scale);

			hkQsTransformf* transform = pose->AccessBoneLocalSpace(this.Id.BoneIndex);
			transform->Translation.Set(newTransform.Translation);
			transform->Rotation.Set(newTransform.Rotation);
			transform->Scale.Set(newTransform.Scale);
		}

		return skeleton;
	}

	public void Dispose()
	{
		this.Parent = null;
		this.IsValid = false;
	}
}
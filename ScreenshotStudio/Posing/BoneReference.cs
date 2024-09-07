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

public class BoneReference(string? name, BoneId id)
{
	public readonly string? Name = name;
	public readonly BoneId Id = id;

	public Vector3 LastCharacterTranslation;
	public Quaternion LastCharacterRotation;
	public Vector3 LastCharacterScale;

	public hkQsTransformf LiveTransform;
	public hkQsTransformf? Transform = null;
	public hkQsTransformf? NextModelSpaceTransform;
	public hkQsTransformf? LocalSpaceTransform = null;

	public hkQsTransformf ReferenceTransform;
	public hkQsTransformf NextReferenceRelativeTransform;

	public BoneReference? Parent;
	public bool IsValid = true;

	public bool Locked { get; set; } = false;
	public bool ForceRef { get; set; } = false;

	public void SetToReference()
	{
		this.Locked = true;

		if (this.LocalSpaceTransform == null)
			throw new Exception("Cannot set bone to reference before it has been ticked");

		var newTransform = this.ReferenceTransform;
		newTransform.Subtract(this.LocalSpaceTransform.Value);
		this.Transform = newTransform;
	}

	public void Clear()
	{
		this.Parent = null;
		this.IsValid = false;
	}

	public unsafe Skeleton* Tick()
	{
		Threads.VerifyFrameworkThread();

		if (!this.IsValid)
			return null;

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

		this.ReferenceTransform = pose->Skeleton->ReferencePose[this.Id.BoneIndex];

		// Get a new copy of the live transforms
		this.LiveTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

		if (this.LocalSpaceTransform == null || !this.Locked)
			this.LocalSpaceTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

		if (this.NextModelSpaceTransform != null)
		{
			hkQsTransformf newTransform = this.NextModelSpaceTransform.Value;
			////newTransform.Translation.Subtract(this.LiveTransform.Translation);
			newTransform.Rotation.Divide(this.LiveTransform.Rotation);
			////newTransform.Scale.Subtract(this.LiveTransform.Scale);

			this.Transform = newTransform;
			this.NextModelSpaceTransform = null;
		}

		if (this.Transform != null)
		{
			if (this.Locked)
			{
				hkQsTransformf newTransform = (hkQsTransformf)this.LocalSpaceTransform;
				newTransform.Translation.Add(this.Transform.Value.Translation);
				newTransform.Rotation.Multiply(this.Transform.Value.Rotation);
				newTransform.Scale.Add(this.Transform.Value.Scale);

				hkQsTransformf* transform = pose->AccessBoneLocalSpace(this.Id.BoneIndex);
				transform->Translation.Set(newTransform.Translation);
				transform->Rotation.Set(newTransform.Rotation);
				transform->Scale.Set(newTransform.Scale);
			}
			else
			{
				// Modify the live transform
				hkQsTransformf* transform = pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
				transform->Translation.Add(this.Transform.Value.Translation);
				transform->Rotation.Multiply(this.Transform.Value.Rotation);
				transform->Scale.Add(this.Transform.Value.Scale);
			}
		}

		return skeleton;
	}
}
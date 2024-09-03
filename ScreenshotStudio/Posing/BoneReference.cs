namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Xml.Linq;

public class BoneReference : IDisposable
{
	public readonly string? Name;
	public readonly BoneId Id;

	public Vector3 LastCharacterTranslation;
	public Quaternion LastCharacterRotation;
	public Vector3 LastCharacterScale;

	public hkQsTransformf LastTransform;
	public hkQsTransformf? CurrentTransform = null;
	public Modes Mode;
	public hkQsTransformf? LastLocalTransform = null;

	public hkVector4f? NextAbsoluteTranslation;
	public hkQuaternionf? NextAbsoluteRotation;
	public hkVector4f? NextAbsoluteScale;

	public hkQsTransformf ReferenceTransform;

	public BoneReference? Parent;
	public bool IsValid = true;

	public BoneReference(string? name, BoneId id)
	{
		this.Name = name;
		this.Id = id;
	}

	public enum Modes
	{
		/// <summary>
		/// modifies the bone by adding the CurrentTransform to the games live transform.
		/// </summary>
		Relative,

		/// <summary>
		/// Modifies the bone by adding the CurrentTransform to the last game transform read before locking.
		/// </summary>
		Locked_Relative,

		/// <summary>
		/// Modifies the bone by setting the live transform to CurrentTransform, calculating the relative value, and swapping to relative_locked.
		/// </summary>
		Absolute,

		/// <summary>
		/// Sets the pose to reference mode.
		/// </summary>
		Reference,

		/// <summary>
		/// An absolute transform, but relative to the skeletons bind or reference pose, allows for transfers between races without distortion.
		/// </summary>
		Reference_Relative,
	}

	public unsafe Skeleton* ApplyTransform()
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

		if (this.Mode == Modes.Absolute)
		{
			// Get a new copy of the live transforms
			this.LastTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			this.LastLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			hkQsTransformf newTransform = default;
			newTransform = default;
			newTransform.Rotation = HkQuaternionExtensions.Identity;

			if (this.NextAbsoluteTranslation != null)
			{
				newTransform.Translation = this.NextAbsoluteTranslation.Value;
				newTransform.Translation.Subtract(this.LastTransform.Translation);
			}

			if (this.NextAbsoluteRotation != null)
			{
				newTransform.Rotation = this.NextAbsoluteRotation.Value;
				newTransform.Rotation.Divide(this.LastTransform.Rotation);
			}

			if (this.NextAbsoluteScale != null)
			{
				newTransform.Scale = this.NextAbsoluteScale.Value;
				newTransform.Scale.Subtract(this.LastTransform.Scale);
			}

			this.CurrentTransform = newTransform;

			this.Mode = Modes.Locked_Relative;
			this.NextAbsoluteTranslation = null;
			this.NextAbsoluteRotation = null;
			this.NextAbsoluteScale = null;
		}

		if (this.Mode == Modes.Relative)
		{
			// Get a new copy of the live transforms
			this.LastTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			this.LastLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			// Modify the live transform
			if (this.CurrentTransform != null)
			{
				hkQsTransformf* transform = pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
				transform->Translation.Add(this.CurrentTransform.Value.Translation);
				transform->Rotation.Multiply(this.CurrentTransform.Value.Rotation);
				transform->Scale.Add(this.CurrentTransform.Value.Scale);
			}
		}
		else if (this.Mode == Modes.Locked_Relative)
		{
			if (this.LastLocalTransform == null)
				this.LastLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			hkQsTransformf newTransform = (hkQsTransformf)this.LastLocalTransform;

			if (this.CurrentTransform != null)
			{
				newTransform.Translation.Add(this.CurrentTransform.Value.Translation);
				newTransform.Rotation.Multiply(this.CurrentTransform.Value.Rotation);
				newTransform.Scale.Add(this.CurrentTransform.Value.Scale);
			}

			hkQsTransformf* transform = pose->AccessBoneLocalSpace(this.Id.BoneIndex);
			transform->Translation.Set(newTransform.Translation);
			transform->Rotation.Set(newTransform.Rotation);
			transform->Scale.Set(newTransform.Scale);
		}
		else if (this.Mode == Modes.Reference)
		{
			hkQsTransformf* transform = pose->AccessBoneLocalSpace(this.Id.BoneIndex);
			transform->Translation = this.ReferenceTransform.Translation;
			transform->Rotation = this.ReferenceTransform.Rotation;
			transform->Scale = this.ReferenceTransform.Scale;
		}

		return skeleton;
	}

	public void Dispose()
	{
		this.Parent = null;
		this.IsValid = false;
	}
}
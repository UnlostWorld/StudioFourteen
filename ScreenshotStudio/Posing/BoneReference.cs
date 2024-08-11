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
	public hkQsTransformf CurrentTransform;
	public Modes Mode;
	public hkQsTransformf LastLocalTransform;

	public hkVector4f? NextAbsoluteTranslation;
	public hkQuaternionf? NextAbsoluteRotation;
	public hkVector4f? NextAbsoluteScale;

	public BoneReference? Parent;
	public bool IsValid = true;

	public BoneReference(string? name, BoneId id)
	{
		this.Name = name;
		this.Id = id;

		this.CurrentTransform = default;
		this.CurrentTransform.Rotation = HkQuaternionExtensions.Identity;
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

		if (this.Mode == Modes.Absolute)
		{
			// Get a new copy of the live transforms
			this.LastTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			this.LastLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

			this.CurrentTransform = default;
			this.CurrentTransform.Rotation = HkQuaternionExtensions.Identity;

			if (this.NextAbsoluteTranslation != null)
			{
				this.CurrentTransform.Translation = this.NextAbsoluteTranslation.Value;
				this.CurrentTransform.Translation.Subtract(this.LastTransform.Translation);
			}

			if (this.NextAbsoluteRotation != null)
			{
				this.CurrentTransform.Rotation = this.NextAbsoluteRotation.Value;
				this.CurrentTransform.Rotation.Divide(this.LastTransform.Rotation);
			}

			if (this.NextAbsoluteScale != null)
			{
				this.CurrentTransform.Scale = this.NextAbsoluteScale.Value;
				this.CurrentTransform.Scale.Subtract(this.LastTransform.Scale);
			}

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
			hkQsTransformf* transform = pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			transform->Translation.Add(this.CurrentTransform.Translation);
			transform->Rotation.Multiply(this.CurrentTransform.Rotation);
			transform->Scale.Add(this.CurrentTransform.Scale);
		}
		else if (this.Mode == Modes.Locked_Relative)
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
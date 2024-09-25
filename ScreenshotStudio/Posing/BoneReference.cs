namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Files;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Numerics;
using static ScreenshotStudio.Files.PoseFile;

public class BoneReference(BoneId id, string? name = null)
{
	public readonly BoneId Id = id;

	public Vector3 LastCharacterTranslation;
	public Quaternion LastCharacterRotation;
	public Vector3 LastCharacterScale;

	public hkQsTransformf? ModelSpaceTransform = null;
	public hkQsTransformf? Transform = null;
	public hkQsTransformf? LocalSpaceTransform = null;

	public hkQsTransformf ReferenceTransform;
	public hkQsTransformf NextReferenceRelativeTransform;

	public PoseFile.LegacyBoneTransform? LoadModelSpaceTransform;
	public PoseFile.BoneTransform? LoadRelativeTransform;

	public BoneReference? Parent;
	public bool IsValid = true;

	private string? boneName = name;

	public bool Locked { get; set; } = false;
	public bool ForceRef { get; set; } = false;

	public string? Name
	{
		get => this.boneName;
		set => this.boneName = value;
	}

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

	public PoseFile.BoneTransform? GetLiveReferenceRelativeTransform()
	{
		if (this.LocalSpaceTransform == null)
			return null;

		hkQsTransformf hkReferenceRelativeTransform = this.LocalSpaceTransform.Value;
		if (this.Transform != null)
			hkReferenceRelativeTransform.Add(this.Transform.Value);

		hkReferenceRelativeTransform.Subtract(this.ReferenceTransform);

		BoneTransform referenceRelative = new();
		referenceRelative.Translation = hkReferenceRelativeTransform.Translation.ToVector3();
		referenceRelative.Rotation = hkReferenceRelativeTransform.Rotation.ToQuaternion();
		referenceRelative.Scale = hkReferenceRelativeTransform.Scale.ToVector3();

		return referenceRelative;
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

		// Update or sanity check bone name, useful if the skeleton has changed during posing.
		hkaBone bone = pose->Skeleton->Bones[this.Id.BoneIndex];
		if (this.boneName == null)
		{
			this.boneName = bone.Name.String;
		}
		else
		{
			if (bone.Name.String != this.Name)
			{
				return null;
			}
		}

		this.ReferenceTransform = pose->Skeleton->ReferencePose[this.Id.BoneIndex];

		// Get a new copy of the live transforms
		this.ModelSpaceTransform = *pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

		if (this.LocalSpaceTransform == null || !this.Locked)
			this.LocalSpaceTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);

		if (this.LoadModelSpaceTransform != null)
		{
			hkQsTransformf newTransform = default;
			newTransform.Rotation = HkQuaternionExtensions.Identity;
			if (this.LoadModelSpaceTransform.Position != null)
			{
				newTransform.Translation = this.LoadModelSpaceTransform.Position.Value.ToHkVector();
				newTransform.Translation.Subtract(this.ModelSpaceTransform.Value.Translation);
			}

			if (this.LoadModelSpaceTransform.Rotation != null)
			{
				newTransform.Rotation = this.LoadModelSpaceTransform.Rotation.Value.ToHkQuaternion();
				newTransform.Rotation.Divide(this.ModelSpaceTransform.Value.Rotation);
			}

			if (this.LoadModelSpaceTransform.Scale != null)
			{
				newTransform.Scale = this.LoadModelSpaceTransform.Scale.Value.ToHkVector();
				newTransform.Scale.Subtract(this.ModelSpaceTransform.Value.Scale);
			}

			this.Transform = newTransform;
			this.LoadModelSpaceTransform = null;
		}

		if (this.LoadRelativeTransform != null)
		{
			hkQsTransformf newTransform = default;
			newTransform.Rotation = HkQuaternionExtensions.Identity;

			if (this.LoadRelativeTransform.Translation != null)
			{
				newTransform.Translation = this.ReferenceTransform.Translation;
				newTransform.Translation.Add(this.LoadRelativeTransform.Translation.Value.ToHkVector());
				newTransform.Translation.Subtract(this.LocalSpaceTransform.Value.Translation);
			}

			if (this.LoadRelativeTransform.Rotation != null)
			{
				newTransform.Rotation = this.ReferenceTransform.Rotation;
				newTransform.Rotation.Multiply(this.LoadRelativeTransform.Rotation.Value.ToHkQuaternion());
				newTransform.Rotation.Divide(this.LocalSpaceTransform.Value.Rotation);
			}

			if (this.LoadRelativeTransform.Scale != null)
			{
				newTransform.Scale = this.ReferenceTransform.Scale;
				newTransform.Scale.Add(this.LoadRelativeTransform.Scale.Value.ToHkVector());
				newTransform.Scale.Subtract(this.LocalSpaceTransform.Value.Scale);
			}

			this.Transform = newTransform;
			this.LoadRelativeTransform = null;
		}

		if (this.Transform != null)
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

		return skeleton;
	}
}
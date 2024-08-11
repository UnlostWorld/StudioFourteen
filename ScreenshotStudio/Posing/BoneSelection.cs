namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

public class BoneSelection : SelectionBase
{
	private readonly List<BoneId> boneIds;
	private readonly List<BoneId> parentBoneIds;
	private readonly List<BoneReference> bones = new();
	private BoneReference? bone;

	public BoneSelection(List<BoneId> bones, List<BoneId> parents, string name)
	{
		this.BoneName = name;
		this.boneIds = bones;
		this.parentBoneIds = parents;
	}

	public BoneSelection(BoneId bone, BoneId parent, string name)
	{
		this.BoneName = name;
		this.boneIds = [bone];
		this.parentBoneIds = [parent];
	}

	public BoneSelection(BoneId bone, string name)
	{
		this.BoneName = name;
		this.boneIds = [bone];
		this.parentBoneIds = [];
	}

	public override string Name => Resources.Find($"LOC_Bone_{this.BoneName}", this.BoneName);
	public override string? Subtitle => this.BoneName;
	public string BoneName { get; init; }
	public IReadOnlyCollection<BoneId> BoneIds => this.boneIds.AsReadOnly();
	public IReadOnlyCollection<BoneId> ParentBoneIds => this.parentBoneIds.AsReadOnly();

	public BoneReference Bone
	{
		get
		{
			if (this.bone == null)
				throw new Exception("Attempt to access bone selection transform before a bone has been assigned");

			return this.bone;
		}
	}

	public override bool LockTransform
	{
		get => this.bone?.Mode == BoneReference.Modes.Locked_Relative;
		set => this.Bone.Mode = value ? BoneReference.Modes.Locked_Relative : BoneReference.Modes.Relative;
	}

	public override Vector3 LocalTranslation
	{
		get => this.Transform.Translation.ToVector3();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Translation.FromVector3(value);
			this.Transform = transform;
		}
	}

	public override Quaternion LocalRotation
	{
		get => this.Transform.Rotation.ToQuaternion();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Rotation.FromQuaternion(value);
			this.Transform = transform;
		}
	}

	public override Vector3 LocalScale
	{
		get => this.Transform.Scale.ToVector3();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Scale.FromVector3(value);
			this.Transform = transform;
		}
	}

	public override Vector3 WorldTranslation
	{
		get => this.LocalTranslation + this.Bone.LastCharacterTranslation;
		set => this.LocalTranslation = value - this.Bone.LastCharacterTranslation;
	}

	public override Quaternion WorldRotation
	{
		get => this.Bone.LastCharacterRotation * this.LocalRotation;
		set
		{
			value = Quaternion.Conjugate(value);
			value *= this.Bone.LastCharacterRotation;
			value = Quaternion.Conjugate(value);

			this.LocalRotation = value;
		}
	}

	public override Vector3 WorldScale
	{
		get => this.LocalScale + this.Bone.LastCharacterScale;
		set => this.LocalScale = value - this.Bone.LastCharacterScale;
	}

	// TODO: support multiple bone transforms
	private hkQsTransformf Transform
	{
		get
		{
			hkQsTransformf combine = this.Bone.LastTransform;
			combine.Add(this.Bone.CurrentTransform);
			return combine;
		}
		set
		{
			hkQsTransformf separate = value;
			separate.Subtract(this.Bone.LastTransform);
			this.Bone.CurrentTransform = separate;
		}
	}

	public override void Activate()
	{
		this.bones.Clear();
		foreach (BoneId boneId in this.boneIds)
		{
			this.bones.Add(ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId));
		}

		this.bone = this.bones[0];
	}

	public override void Deactivate()
	{
		this.bones.Clear();
		this.bone = null;
	}
}
namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

public class BoneSelection : TransformSelectionBase
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

		this.IsFaceBone = name.StartsWith("j_f_");
	}

	public BoneSelection(BoneId bone, BoneId parent, string name)
	{
		this.BoneName = name;
		this.boneIds = [bone];
		this.parentBoneIds = [parent];

		this.IsFaceBone = name.StartsWith("j_f_");
	}

	public BoneSelection(BoneId bone, string name)
	{
		this.BoneName = name;
		this.boneIds = [bone];
		this.parentBoneIds = [];

		this.IsFaceBone = name.StartsWith("j_f_");
	}

	public override string Name => Resources.Find($"LOC_Bone_{this.BoneName}", this.BoneName);
	public override string? Subtitle => this.BoneName;
	public string BoneName { get; init; }
	public IReadOnlyCollection<BoneId> BoneIds => this.boneIds.AsReadOnly();
	public IReadOnlyCollection<BoneId> ParentBoneIds => this.parentBoneIds.AsReadOnly();

	public bool IsFaceBone { get; private set; }
	public override double TranslationLargeChange => this.IsFaceBone ? 0.01 : 0.1;
	public override double TranslationSmallChange => this.IsFaceBone ? 0.001 : 0.01;
	public override double TranslationRange => this.IsFaceBone ? 0.02 : 0.1;
	public override PoseEditModes DefaultEditMode => this.IsFaceBone ? PoseEditModes.Translation : PoseEditModes.Rotation;

	public override bool LockTransform
	{
		get => this.bone?.Locked == true;
		set
		{
			if (this.bone == null)
				return;

			this.bone.Locked = value;
		}
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
		get => this.LocalTranslation + (this.bone?.LastCharacterTranslation ?? Vector3.Zero);
		set => this.LocalTranslation = value - (this.bone?.LastCharacterTranslation ?? Vector3.Zero);
	}

	public override Quaternion WorldRotation
	{
		get => (this.bone?.LastCharacterRotation ?? Quaternion.Identity) * this.LocalRotation;
		set
		{
			if (this.bone == null)
				return;

			value = Quaternion.Conjugate(value);
			value *= this.bone.LastCharacterRotation;
			value = Quaternion.Conjugate(value);

			this.LocalRotation = value;
		}
	}

	public override Vector3 WorldScale
	{
		get => this.LocalScale + (this.bone?.LastCharacterScale ?? Vector3.One);
		set => this.LocalScale = value - (this.bone?.LastCharacterScale ?? Vector3.One);
	}

	// TODO: support multiple bone transforms
	private hkQsTransformf Transform
	{
		get
		{
			if (this.bone == null)
				return default;

			hkQsTransformf combine = this.bone.LiveTransform;

			if (this.bone.Transform != null)
				combine.Add(this.bone.Transform.Value);

			return combine;
		}
		set
		{
			if (this.bone == null)
				return;

			hkQsTransformf separate = value;
			separate.Subtract(this.bone.LiveTransform);
			this.bone.Transform = separate;
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
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

	public override string Name => Resources.Find($"LOC_Bone_{this.BoneName}", this.BoneName);
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

	public bool UseModValues { get; set; } = false;

	// TODO: support multiple bone transforms
	public hkQsTransformf Transform
	{
		get
		{
			if (this.UseModValues)
			{
				return this.Bone.CurrentTransform;
			}
			else
			{
				hkQsTransformf combine = this.Bone.LastTransform;
				combine.Add(this.Bone.CurrentTransform);
				return combine;
			}
		}
		set
		{
			if (this.UseModValues)
			{
				this.Bone.CurrentTransform = value;
			}
			else
			{
				hkQsTransformf separate = value;
				separate.Subtract(this.Bone.LastTransform);
				this.Bone.CurrentTransform = separate;
			}
		}
	}

	public override bool LockTransform
	{
		get => this.Bone.LockTransform;
		set => this.Bone.LockTransform = value;
	}

	public override Vector3 Translation
	{
		get => this.Transform.Translation.ToVector3();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Translation.FromVector3(value);
			this.Transform = transform;
		}
	}

	public override Quaternion Rotation
	{
		get => this.Transform.Rotation.ToQuaternion();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Rotation.FromQuaternion(value);
			this.Transform = transform;
		}
	}

	public override Vector3 Scale
	{
		get => this.Transform.Scale.ToVector3();
		set
		{
			hkQsTransformf transform = this.Transform;
			transform.Scale.FromVector3(value);
			this.Transform = transform;
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
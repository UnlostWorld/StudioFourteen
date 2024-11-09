namespace StudioFourteen.Posing;

using FFXIVClientStructs;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using StudioFourteen.Files;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using static StudioFourteen.Files.PoseFile;

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
	public override int DecimalPlacesToDisplay => this.IsFaceBone ? 4 : 2;
	public override PoseEditModes DefaultEditMode => this.IsFaceBone ? PoseEditModes.Translation : PoseEditModes.Rotation;
	public override bool CanReset => true;

	public override bool IsReady => this.bone != null && this.bone.LocalSpaceTransform != null;

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

	public override bool CanMirror => true;
	public override MirrorModes MirrorMode { get; set; }

	public override Transform WorldTransform
	{
		get => this.bone?.WorldSpaceTransform ?? default;
		set => this.bone?.SetWorldSpaceTransform(value);
	}

	public override Transform LocalTransform
	{
		get => this.bone?.LocalSpaceTransform ?? default;
		set => this.bone?.SetLocalSpaceTransform(value);
	}

	public Transform ReferenceRelativeTransform
	{
		get => this.bone?.ReferenceRelativeTransform ?? default;
		set => this.bone?.SetReferenceRelativeTransform(value);
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

	public void SetReferenceTransform(Transform referenceTransform)
	{
		BoneTransform transform = new BoneTransform();
		transform.Translation = referenceTransform.Translation;
		transform.Rotation = Quaternion.Normalize(referenceTransform.Rotation);
		transform.Scale = referenceTransform.Scale;
		this.SetReferenceTransform(transform);
	}

	public void SetReferenceTransform(BoneTransform referenceTransform)
	{
		BoneTransform mirrorReferenceTransform = referenceTransform.Flip(this.MirrorMode);

		foreach (BoneReference boneReference in this.bones)
		{
			boneReference.SetReferenceRelativeTransform(referenceTransform);

			if (this.MirrorMode != MirrorModes.None && boneReference.Mirror != null)
			{
				boneReference.Mirror.SetReferenceRelativeTransform(mirrorReferenceTransform);
			}
		}
	}

	public override void Reset()
	{
		foreach(BoneReference bone in this.bones)
		{
			bone.Reset();

			if (this.MirrorMode != MirrorModes.None && bone.Mirror != null)
			{
				bone.Mirror.Reset();
			}
		}
	}

	public override bool Equals(SelectionBase? other)
	{
		if (other is not BoneSelection otherBone)
			return false;

		if (this.boneIds.Count != otherBone.boneIds.Count)
			return false;

		foreach(BoneId id in this.boneIds)
		{
			if (!otherBone.boneIds.Contains(id))
			{
				return false;
			}
		}

		return true;
	}
}
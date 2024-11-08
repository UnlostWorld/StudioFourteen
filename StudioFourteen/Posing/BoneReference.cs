namespace StudioFourteen.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Numerics;

public class BoneReference(BoneId id, string? name = null)
{
	public readonly BoneId Id = id;

	public Transform? LastCharacterTransform;

	public Transform? ModelSpaceTransform = null;
	public Transform? Transform = null;
	public Transform? LocalSpaceTransform = null;

	public Transform ReferenceTransform;
	public Transform NextReferenceRelativeTransform;

	public BoneTransform? LoadModelSpaceTransform;
	public BoneTransform? LoadRelativeTransform;

	public BoneReference? Parent;
	public BoneReference? Mirror;
	public bool IsValid = true;

	private string? boneName = name;
	private string? mirrorBoneName;
	private bool hasCheckedMirror = false;

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
		newTransform -= (Transform)this.LocalSpaceTransform;
		this.Transform = newTransform;
	}

	public void Clear()
	{
		this.Parent = null;
		this.Mirror = null;
		this.IsValid = false;
	}

	public BoneTransform? GetLiveReferenceRelativeTransform()
	{
		return null;
		/*if (this.LocalSpaceTransform == null)
			return null;

		hkQsTransformf hkReferenceRelativeTransform = this.LocalSpaceTransform.Value;
		if (this.Transform != null)
			hkReferenceRelativeTransform.Add(this.Transform.Value);

		hkReferenceRelativeTransform.Subtract(this.ReferenceTransform);

		BoneTransform referenceRelative = new();
		referenceRelative.Translation = hkReferenceRelativeTransform.Translation.ToVector3();
		referenceRelative.Rotation = hkReferenceRelativeTransform.Rotation.ToQuaternion();
		referenceRelative.Scale = hkReferenceRelativeTransform.Scale.ToVector3();

		return referenceRelative;*/
	}

	public unsafe void FinalizeBones()
	{
	}

	public unsafe Skeleton* Tick()
	{
		Threads.VerifyFrameworkThread();

		if (!this.IsValid)
			return null;

		if (!this.Id.Resolve(out Character* pCharacter, out Skeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
			return null;

		// Begin updating transforms
		this.ReferenceTransform = pPose->Skeleton->ReferencePose[this.Id.BoneIndex];

		// Get a new copy of the live transforms
		if (this.ModelSpaceTransform == null || !this.Locked)
			this.ModelSpaceTransform = *pPose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);

		if (this.LocalSpaceTransform == null || !this.Locked)
			this.LocalSpaceTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);

		Transform characterTransform = default;
		characterTransform.Translation = pCharacter->DrawObject->Position;
		characterTransform.Rotation = pCharacter->DrawObject->Rotation;
		characterTransform.Scale = pCharacter->DrawObject->Scale;
		this.LastCharacterTransform = characterTransform;

		// Update or sanity check bone name, useful if the skeleton has changed during posing.
		hkaBone bone = pPose->Skeleton->Bones[this.Id.BoneIndex];
		if (this.boneName == null)
		{
			this.boneName = bone.Name.String;
			this.hasCheckedMirror = false;
		}
		else
		{
			if (bone.Name.String != this.Name)
			{
				return null;
			}
		}

		// Get our Mirror bone
		if (!this.hasCheckedMirror)
		{
			if (this.boneName != null && this.mirrorBoneName == null)
				this.mirrorBoneName = PoseService.GetMirrorBoneName(this.boneName);

			if (this.Mirror == null && this.mirrorBoneName != null)
			{
				int boneCount = pPose->Skeleton->Bones.Length;
				for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
				{
					hkaBone testBone = pPose->Skeleton->Bones[boneIdx];
					string? boneName = testBone.Name.String;
					if (boneName == this.mirrorBoneName)
					{
						BoneId mirrorBoneId = new(this.Id.ObjectTableIndex, this.Id.PartialSkeletonIndex, this.Id.PoseIndex, boneIdx);
						this.Mirror = ServiceManager.Instance.Pose.GetOrCreateBoneReference(mirrorBoneId);

						/*if (this.Mirror.MirrorMode != MirrorModes.None && this.MirrorMode == MirrorModes.None)
						{
							this.MirrorMode = this.Mirror.MirrorMode;
						}
						else if (this.MirrorMode != MirrorModes.None && this.Mirror.MirrorMode == MirrorModes.None)
						{
							this.Mirror.MirrorMode = this.MirrorMode;
						}*/
					}
				}
			}

			this.hasCheckedMirror = true;
		}

		/*if (this.LoadModelSpaceTransform != null)
		{
			hkQsTransformf* boneModelTransform = pose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);

			if (this.LoadModelSpaceTransform.Translation != null)
				boneModelTransform->Translation.Set(this.LoadModelSpaceTransform.Translation.Value.ToHkVector());

			if (this.LoadModelSpaceTransform.Rotation != null)
				boneModelTransform->Rotation.Set(this.LoadModelSpaceTransform.Rotation.Value.ToHkQuaternion());

			if (this.LoadModelSpaceTransform.Scale != null)
				boneModelTransform->Scale.Set(this.LoadModelSpaceTransform.Scale.Value.ToHkVector());

			hkQsTransformf oldBoneLocalTransform = this.LocalSpaceTransform.Value;
			hkQsTransformf newBoneLocalTransform = *pose->AccessBoneLocalSpace(this.Id.BoneIndex);
			newBoneLocalTransform.Subtract(oldBoneLocalTransform);

			this.Transform = newBoneLocalTransform;
			this.LoadRelativeTransform = null;
			this.LoadModelSpaceTransform = null;
		}*/

		/*if (this.LoadRelativeTransform != null)
		{
			Transform newTransform = default;

			if (this.LoadRelativeTransform.Translation != null)
			{
				newTransform.Translation = this.ReferenceTransform.Translation;
				newTransform.Translation += this.LoadRelativeTransform.Translation.Value;
				newTransform.Translation -= this.LocalSpaceTransform.Value.Translation;
			}

			if (this.LoadRelativeTransform.Rotation != null)
			{
				newTransform.Rotation = this.ReferenceTransform.Rotation * (Quaternion)this.LoadRelativeTransform.Rotation;
				newTransform.Rotation /= this.LocalSpaceTransform.Value.Rotation;
			}

			if (this.LoadRelativeTransform.Scale != null)
			{
				newTransform.Scale = this.ReferenceTransform.Scale;
				newTransform.Scale += this.LoadRelativeTransform.Scale.Value;
				newTransform.Scale -= this.LocalSpaceTransform.Value.Scale;
			}

			this.Transform = newTransform;
			this.LoadRelativeTransform = null;
		}*/

		if (this.Transform != null)
		{
			Transform newTransform = (Transform)this.Transform + (Transform)this.LocalSpaceTransform;

			hkQsTransformf* pTransform = pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			pTransform->Translation.Set(newTransform.Translation);
			pTransform->Rotation.Set(newTransform.Rotation);
			pTransform->Scale.Set(newTransform.Scale);
		}

		return pSkeleton;
	}
}

#pragma warning disable
public struct Transform
{
	public Vector3 Translation = Vector3.Zero;
	public Quaternion Rotation = Quaternion.Identity;
	public Vector3 Scale = Vector3.Zero;

	public Transform()
	{
	}

	public static implicit operator Transform(hkQsTransformf transform)
	{
		Transform t = default;
		t.Translation = transform.Translation.ToVector3();
		t.Rotation = transform.Rotation.ToQuaternion();
		t.Scale = transform.Scale.ToVector3();
		return t;
	}

	public static implicit operator hkQsTransformf(Transform transform)
	{
		hkQsTransformf t = default;
		t.Translation = transform.Translation.ToHkVector();
		t.Rotation = transform.Rotation.ToHkQuaternion();
		t.Scale = transform.Scale.ToHkVector();
		return t;
	}

	public static Transform operator +(Transform left, Transform right)
	{
		Transform t = default;
		t.Translation = left.Translation + right.Translation;
		t.Rotation = Quaternion.Normalize(left.Rotation * right.Rotation);
		t.Scale = left.Scale * right.Scale;
		return t;
	}

	public static Transform operator -(Transform left, Transform right)
	{
		Transform t = default;
		t.Translation = left.Translation - right.Translation;
		t.Rotation = Quaternion.Normalize(left.Rotation / right.Rotation);
		t.Scale = left.Scale / right.Scale;
		return t;
	}

	public static bool operator !=(Transform left, Transform right)
	{
		return !(left == right);
	}

	public static bool operator ==(Transform left, Transform right)
	{
		return left.Translation == right.Translation
			&& left.Rotation == right.Rotation
			&& left.Scale == right.Scale;
	}
}
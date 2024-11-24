// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;
using StudioFourteen.Utilities;
using System;
using System.Diagnostics;
using System.Numerics;
using WpfUtils.Animation;

public class BoneReference(BoneId id, string? name = null)
{
	public readonly BoneId Id = id;

	public BoneReference? Parent;
	public BoneReference? Mirror;
	public bool IsValid = true;

	private const float PoseBlendTimeMs = 250;
	private readonly Stopwatch blendTime = new();
	private readonly EasingFunctionBase blendEase = new SineEase();
	private bool blendOnLoad = false;
	private bool blendOnUnload = false;
	private Transform? fromTransform;
	private Transform? toTransform;

	private Transform? baseLocalTransform;
	private Transform? loadLocalSpaceTransform;
	private BoneTransform? loadModelSpaceBoneTransform;
	private Transform? loadModelSpaceTransform;
	private Transform? loadReferenceRelativeTransform;
	private string? boneName = name;
	private string? mirrorBoneName;
	private bool hasCheckedMirror = false;

	public Transform? Transform { get; private set; }
	public Transform? LocalSpaceTransform { get; private set; }
	public Transform? ModelSpaceTransform { get; private set; }
	public Transform? ModelTransform { get; private set; }
	public Transform? ReferenceRelativeTransform { get; private set; }
	public Transform? ReferenceTransform { get; private set; }

	public bool Locked { get; set; } = false;
	public bool ForceRef { get; set; } = false;
	public MirrorModes MirrorMode { get; set; }

	public string? Name
	{
		get => this.boneName;
		set => this.boneName = value;
	}

	public bool IsBlending => this.blendOnLoad || this.blendOnUnload;
	public bool IsBlendingOut => this.blendOnUnload;

	public void SetToReference()
	{
		this.Locked = true;

		if (this.LocalSpaceTransform == null)
			throw new Exception("Cannot set bone to reference before it has been ticked");

		var newTransform = this.ReferenceTransform;
		newTransform /= (Transform)this.LocalSpaceTransform;
		this.Transform = newTransform;
	}

	public void Dispose()
	{
		this.Parent = null;
		this.Mirror = null;
		this.IsValid = false;
	}

	public void SetLocalSpaceTransform(Transform localSpaceTransform)
	{
		this.ReverseMirror();
		this.loadLocalSpaceTransform = localSpaceTransform;
	}

	public void SetModelSpaceTransform(Transform modelSpaceTransform)
	{
		this.ReverseMirror();
		this.loadModelSpaceTransform = modelSpaceTransform;
	}

	public void SetModelSpaceTransform(BoneTransform modelSpaceTransform)
	{
		this.ReverseMirror();
		this.loadModelSpaceBoneTransform = modelSpaceTransform;
	}

	public void SetReferenceRelativeTransform(Transform referenceRelativeTransform)
	{
		this.ReverseMirror();
		this.loadReferenceRelativeTransform = referenceRelativeTransform;
	}

	public void SetReferenceRelativeTransform(BoneTransform referenceRelativeTransform)
	{
		this.ReverseMirror();
		Transform relativeTransform = default;
		relativeTransform.Translation = referenceRelativeTransform.Translation ?? Vector3.Zero;
		relativeTransform.Rotation = referenceRelativeTransform.Rotation ?? Quaternion.Identity;
		relativeTransform.Scale = referenceRelativeTransform.Scale ?? Vector3.One;
		this.loadReferenceRelativeTransform = relativeTransform;
	}

	public void Reset(bool immediate)
	{
		this.MirrorMode = MirrorModes.None;

		if (immediate || this.Transform == null)
		{
			this.fromTransform = null;
			this.toTransform = null;
			this.Transform = null;
			this.blendTime.Stop();
		}
		else
		{
			this.fromTransform = this.Transform;
			this.toTransform = new Posing.Transform();
			this.blendOnUnload = true;
			this.blendTime.Restart();
		}
	}

	public unsafe void FinalizeBones()
	{
		Threads.VerifyFrameworkThread();

		if (!this.IsValid)
			return;

		if (!this.Id.Resolve(out Character* pCharacter, out Skeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
			return;

		this.ReferenceTransform = pPose->Skeleton->ReferencePose[this.Id.BoneIndex];
		this.ModelSpaceTransform = *pPose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
		this.LocalSpaceTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);

		Transform characterTransform = default;
		characterTransform.Translation = pCharacter->DrawObject->Position;
		characterTransform.Rotation = pCharacter->DrawObject->Rotation;
		characterTransform.Scale = pCharacter->DrawObject->Scale;
		this.ModelTransform = characterTransform;
		this.ReferenceRelativeTransform = (Transform)this.LocalSpaceTransform / (Transform)this.ReferenceTransform;
	}

	public unsafe Skeleton* Tick()
	{
		Threads.VerifyFrameworkThread();

		if (!this.IsValid)
			return null;

		if (!this.Id.Resolve(out Character* pCharacter, out Skeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
			return null;

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

		// Get a new copy of the live transform
		if (this.baseLocalTransform == null || !this.Locked)
			this.baseLocalTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);

		// apply model space changes
		if (this.loadModelSpaceTransform != null)
		{
			hkQsTransformf* boneModelTransform = pPose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);
			boneModelTransform->Translation.Set(this.loadModelSpaceTransform.Value.Translation.ToHkVector());
			boneModelTransform->Rotation.Set(this.loadModelSpaceTransform.Value.Rotation.ToHkQuaternion());
			boneModelTransform->Scale.Set(this.loadModelSpaceTransform.Value.Scale.ToHkVector());

			this.loadLocalSpaceTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			this.loadModelSpaceTransform = null;
			this.blendOnLoad = false;
		}

		// apply model space bone changes (legacy pose file format)
		if (this.loadModelSpaceBoneTransform != null)
		{
			hkQsTransformf* boneModelTransform = pPose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.Propagate);

			if (this.loadModelSpaceBoneTransform.Translation != null)
				boneModelTransform->Translation.Set((Vector3)this.loadModelSpaceBoneTransform.Translation);

			if (this.loadModelSpaceBoneTransform.Rotation != null)
				boneModelTransform->Rotation.Set((Quaternion)this.loadModelSpaceBoneTransform.Rotation);

			if (this.loadModelSpaceBoneTransform.Scale != null)
				boneModelTransform->Scale.Set((Vector3)this.loadModelSpaceBoneTransform.Scale);

			this.loadLocalSpaceTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			this.loadModelSpaceBoneTransform = null;
			this.blendOnLoad = false;
		}

		// Apply reference relative changes.
		if (this.loadReferenceRelativeTransform != null && this.ReferenceTransform != null)
		{
			this.blendOnLoad = true;

			this.loadLocalSpaceTransform = (Transform)this.loadReferenceRelativeTransform * (Transform)this.ReferenceTransform;
			this.loadReferenceRelativeTransform = null;
		}

		// apply local space changes.
		// and set up blend if desired.
		if (this.loadLocalSpaceTransform != null)
		{
			if (this.blendOnLoad)
			{
				if (this.Transform == null)
					this.Transform = new Posing.Transform();

				this.fromTransform = this.Transform;
				this.blendTime.Restart();
			}
			else
			{
				this.fromTransform = null;
			}

			this.toTransform = this.loadLocalSpaceTransform / this.baseLocalTransform;
			this.loadLocalSpaceTransform = null;
		}

		// Apply blend to the Transform.
		if (this.toTransform != null)
		{
			if (this.fromTransform != null && (this.blendOnLoad || this.blendOnUnload))
			{
				float p = this.blendTime.ElapsedMilliseconds / PoseBlendTimeMs;
				p = Math.Clamp(p, 0, 1);
				p = this.blendEase.Ease(p, EasingFunctionBase.EasingModes.EaseInOut);
				this.Transform = Posing.Transform.Lerp(this.fromTransform.Value, this.toTransform.Value, p);

				if (p >= 1)
				{
					if (this.blendOnUnload)
					{
						this.Transform = null;
						this.Locked = false;
					}
					else
					{
						this.Transform = this.toTransform;
					}

					this.toTransform = null;
					this.fromTransform = null;
					this.blendTime.Stop();
					this.blendOnLoad = false;
					this.blendOnUnload = false;
				}
			}
			else
			{
				this.Transform = this.toTransform;
				this.toTransform = null;
				this.blendOnLoad = false;
				this.blendOnUnload = false;
			}
		}

		// Apply transform to live.
		if (this.Transform != null)
		{
			Transform newTransform = (Transform)this.Transform * (Transform)this.baseLocalTransform;

			// do not allow bones to scale to 0. bad things happen.
			newTransform.Scale = Vector3.Max(newTransform.Scale, new Vector3(0.1f, 0.1f, 0.1f));

			hkQsTransformf* pTransform = pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			pTransform->Translation.Set(newTransform.Translation);
			pTransform->Rotation.Set(newTransform.Rotation);
			pTransform->Scale.Set(newTransform.Scale);
		}

		// Apply mirroring
		if (this.Mirror != null && (this.MirrorMode != MirrorModes.None && this.MirrorMode != MirrorModes.Receiving))
		{
			Transform localTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			Transform mirrorTransform = FlipUtility.Flip(localTransform, this.MirrorMode);
			this.Mirror.loadLocalSpaceTransform = mirrorTransform;
			this.Mirror.MirrorMode = MirrorModes.Receiving;
		}

		return pSkeleton;
	}

	private void ReverseMirror()
	{
		if (this.MirrorMode != MirrorModes.Receiving)
			return;

		if (this.Mirror == null)
			return;

		MirrorModes sourceMode = this.Mirror.MirrorMode;

		this.MirrorMode = sourceMode;
		this.Mirror.MirrorMode = MirrorModes.Receiving;
	}
}
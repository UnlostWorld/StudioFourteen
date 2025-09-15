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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;
using WpfUtils.Animation;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using XivSkeleton = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

public class BoneReference
{
	public static readonly Vector3 MinScale = new Vector3(0.1f, 0.1f, 0.1f);
	public static readonly Vector3 MaxScale = new Vector3(1000, 1000, 1000);
	public static readonly Vector3 MinTranslate = new Vector3(-10, -10, -10);
	public static readonly Vector3 MaxTranslate = new Vector3(10, 10, 10);

	public readonly BoneId Id;
	public readonly BoneId? ParentId;
	public readonly string BoneName;

	public BoneReference? Mirror;
	public bool IsValid = true;

	private const float PoseBlendTimeMs = 250;

	private readonly Queue<BoneOperationBase> operations = new();

	private readonly Skeleton skeleton;
	private readonly Stopwatch blendTime = new();
	private readonly EasingFunctionBase blendEase = new SineEase();
	private bool blendOnLoad = false;
	private bool blendOnUnload = false;
	private Transform? fromTransform;
	private Transform? toTransform;

	private Transform? baseLocalTransform;

	private string? mirrorBoneName;
	private bool hasCheckedMirror = false;
	private bool isDecomposeError = false;

	public BoneReference(Skeleton skeleton, BoneId id, BoneId? parentId, string name)
	{
		this.Id = id;
		this.ParentId = parentId;
		this.BoneName = name;
		this.skeleton = skeleton;
	}

	public Transform? Transform { get; set; }
	public Transform? LocalSpaceTransform { get; private set; }
	public Transform? ModelSpaceTransform { get; private set; }
	public Transform? ModelTransform { get; private set; }
	public Transform? ReferenceRelativeTransform { get; private set; }
	public Transform? ReferenceTransform { get; private set; }

	public bool Locked { get; set; } = false;
	public bool ForceRef { get; set; } = false;
	public MirrorModes MirrorMode { get; set; }

	public bool IsBlending => this.blendOnLoad || this.blendOnUnload;
	public bool IsBlendingOut => this.blendOnUnload;

	public void Perform(BoneOperationBase operation)
	{
		this.operations.Enqueue(operation);
	}

	public void SetToReference()
	{
		this.Locked = true;

		if (this.LocalSpaceTransform == null || this.ReferenceTransform == null)
			throw new Exception("Cannot set bone to reference before it has been ticked");

		Transform? output;
		bool success = this.ReferenceTransform.Value.DivideBy((Transform)this.LocalSpaceTransform, out output);
		this.Transform = output;
	}

	public void Dispose()
	{
		this.Mirror = null;
		this.IsValid = false;
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
			this.toTransform = new Transform();
			this.blendOnUnload = true;
			this.blendTime.Restart();
		}
	}

	public unsafe void FinalizeBones()
	{
		TickService.VerifyGameTickThread();

		if (!this.IsValid)
			return;

		if (!this.Id.Resolve(out XivCharacter* pCharacter, out XivSkeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
			return;

		this.ReferenceTransform = pPose->Skeleton->ReferencePose[this.Id.BoneIndex];
		this.ModelSpaceTransform = *pPose->AccessBoneModelSpace(this.Id.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

		float scale = pCharacter->GetCharacterScale();
		this.ModelTransform = StudioFourteen.Transform.FromTRS(
			pCharacter->DrawObject->Position,
			pCharacter->DrawObject->Rotation,
			pCharacter->DrawObject->Scale * scale);

		if (this.LocalSpaceTransform != null)
		{
			Transform? output;
			bool success = this.LocalSpaceTransform.Value.DivideBy((Transform)this.ReferenceTransform, out output);
			this.ReferenceRelativeTransform = output;
		}
	}

	public unsafe XivSkeleton* Tick()
	{
		TickService.VerifyGameTickThread();

		if (!this.IsValid)
			return null;

		if (!this.Id.Resolve(out XivCharacter* pCharacter, out XivSkeleton* pSkeleton, out PartialSkeleton* pPartialSkeleton, out hkaPose* pPose))
			return null;

		if (this.Id.BoneIndex >= pPose->Skeleton->Bones.Length)
			return null;

		// Update or sanity check bone name, useful if the skeleton has changed during posing.
		hkaBone bone = pPose->Skeleton->Bones[this.Id.BoneIndex];
		if (bone.Name.String != this.BoneName)
			return null;

		// Get our Mirror bone
		if (!this.hasCheckedMirror)
		{
			if (this.BoneName != null && this.mirrorBoneName == null)
				this.mirrorBoneName = SkeletonService.GetMirrorBoneName(this.BoneName);

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
						this.Mirror = this.skeleton.FindBoneReference(mirrorBoneId);

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

		while (this.operations.Count > 0)
		{
			this.ReverseMirror();

			BoneOperationBase operation = this.operations.Dequeue();

			if (operation.Blend)
			{
				if (this.Transform == null)
					this.Transform = new StudioFourteen.Transform();

				this.fromTransform = this.Transform;
				this.blendTime.Restart();
				this.blendOnLoad = true;
			}

			this.toTransform = operation.Apply(this, (Transform)this.baseLocalTransform, pPose, this.Id.BoneIndex);
		}

		// Apply blend to the Transform.
		if (this.toTransform != null)
		{
			if (this.fromTransform != null && (this.blendOnLoad || this.blendOnUnload))
			{
				float p = this.blendTime.ElapsedMilliseconds / PoseBlendTimeMs;
				p = Math.Clamp(p, 0, 1);
				p = this.blendEase.Ease(p, EasingFunctionBase.EasingModes.EaseInOut);
				this.Transform = StudioFourteen.Transform.Lerp(this.fromTransform.Value, this.toTransform.Value, p);

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
		this.LocalSpaceTransform = this.baseLocalTransform;
		if (this.Transform != null)
		{
			hkQsTransformf* pTransform = pPose->AccessBoneLocalSpace(this.Id.BoneIndex);

			Transform newTransform = (Transform)this.Transform * (Transform)this.baseLocalTransform;
			bool success = newTransform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale);
			if (success)
			{
				this.isDecomposeError = false;

				pTransform->Translation.Set(Vector3.Clamp(translation, MinTranslate, MaxTranslate));
				pTransform->Rotation.Set(rotation);
				pTransform->Scale.Set(Vector3.Clamp(scale, MinScale, MaxScale));
			}
			else
			{
				if (!this.isDecomposeError)
					Logging.Shared.Warning($"Failed to decompose transform for bone {this.BoneName}");

				this.isDecomposeError = true;
			}

			this.LocalSpaceTransform = *pTransform;
		}
		else if (this.Locked)
		{
			hkQsTransformf* pTransform = pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			pTransform->Translation.Set(this.baseLocalTransform.Value.Translation);
			pTransform->Rotation.Set(this.baseLocalTransform.Value.Rotation);
			pTransform->Scale.Set(this.baseLocalTransform.Value.Scale);

			this.LocalSpaceTransform = *pTransform;
		}

		// Apply mirroring
		if (this.Mirror != null && this.MirrorMode != MirrorModes.None && this.MirrorMode != MirrorModes.Receiving)
		{
			Transform localTransform = *pPose->AccessBoneLocalSpace(this.Id.BoneIndex);
			Transform mirrorTransform = FlipUtility.Flip(localTransform, this.MirrorMode);

			this.Mirror.Perform(new LoadLocalSpaceTransformOperation(mirrorTransform));
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
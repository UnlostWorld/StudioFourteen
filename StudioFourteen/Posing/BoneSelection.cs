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
using StudioFourteen.Gizmos;
using System.Collections.Generic;
using System.Numerics;

public class BoneSelection : TransformSelectionBase
{
	// Bones default to rotation, so just set any translation or scale modes here.
	private static readonly Dictionary<string, GizmoTypes> DefaultBoneGizmos = new()
	{
		// Face
		{ "j_f_mmayu_l", GizmoTypes.Translation },
		{ "j_f_mayu_l", GizmoTypes.Translation },
		{ "j_f_miken_01_l", GizmoTypes.Translation },
		{ "j_f_miken_02_l", GizmoTypes.Translation },
		{ "j_f_dmiken_02_l", GizmoTypes.Translation },
		{ "j_f_uhana", GizmoTypes.Translation },
		{ "j_f_hana_l", GizmoTypes.Translation },
		{ "j_f_dmemoto_l", GizmoTypes.Translation },
		{ "j_f_hoho_l", GizmoTypes.Translation },
		{ "j_f_dhoho_l", GizmoTypes.Translation },
		{ "j_f_shoho_l", GizmoTypes.Translation },

		// Mouth
		{ "j_f_ulip_01_l", GizmoTypes.Translation },
		{ "j_f_ulip_02_l", GizmoTypes.Translation },
		{ "j_f_umlip_01_l", GizmoTypes.Translation },
		{ "j_f_umlip_02_l", GizmoTypes.Translation },
		{ "j_f_uslip_l", GizmoTypes.Translation },
		{ "j_f_dlip_01_l", GizmoTypes.Translation },
		{ "j_f_dlip_02_l", GizmoTypes.Translation },
		{ "j_f_dmlip_01_l", GizmoTypes.Translation },
		{ "j_f_dmlip_02_l", GizmoTypes.Translation },
		{ "j_f_dslip_l", GizmoTypes.Translation },

		// Eyes
		{ "j_f_mabup_03in_l", GizmoTypes.Translation },
		{ "j_f_mabup_02out_l", GizmoTypes.Translation },
		{ "j_f_mabdn_03in_l", GizmoTypes.Translation },
		{ "j_f_mabdn_02out_l", GizmoTypes.Translation },
	};

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
	public override bool CanReset => true;
	public override bool IsReady => this.bone != null && this.bone.LocalSpaceTransform != null;
	public override double GizmoSensitivity => this.IsFaceBone ? 0.05 : 0.5;

	public override GizmoTypes DefaultGizmo
	{
		get
		{
			GizmoTypes gizmo;

			if (DefaultBoneGizmos.TryGetValue(this.BoneName, out gizmo))
			{
				return gizmo;
			}

			string? mirrorName = PoseService.GetMirrorBoneName(this.BoneName);
			if (mirrorName != null)
			{
				if (DefaultBoneGizmos.TryGetValue(mirrorName, out gizmo))
				{
					return gizmo;
				}
			}

			return GizmoTypes.Rotation;
		}
	}

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
	public override MirrorModes MirrorMode
	{
		get
		{
			if (this.bone == null)
				return MirrorModes.None;

			MirrorModes mode = this.bone.MirrorMode;

			if (mode == MirrorModes.Receiving && this.bone.Mirror != null)
				mode = this.bone.Mirror.MirrorMode;

			return mode;
		}
		set
		{
			foreach(BoneReference bone in this.bones)
			{
				bone.MirrorMode = value;
			}
		}
	}

	public override Transform WorldTransform
	{
		get
		{
			if (this.bone == null || this.bone.ModelTransform == null || this.bone.ModelSpaceTransform == null)
				return default;

			return this.bone.ModelSpaceTransform.Value * this.bone.ModelTransform.Value;
		}
		set => this.SetWorldTransform(value);
	}

	public override Transform LocalTransform
	{
		get => this.bone?.LocalSpaceTransform ?? default;
		set => this.SetLocalTransform(value);
	}

	public Transform ReferenceRelativeTransform
	{
		get => this.bone?.ReferenceRelativeTransform ?? default;
		set => this.SetReferenceTransform(value);
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

	public override void Reset()
	{
		foreach(BoneReference bone in this.bones)
		{
			bone.Reset(false);

			if (this.MirrorMode != MirrorModes.None && bone.Mirror != null)
			{
				bone.Mirror.Reset(false);
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

	public void SetWorldTransform(Transform transform)
	{
		if (this.bone == null || this.bone.ModelTransform == null)
			return;

		Transform modelSpaceTransform = transform / this.bone.ModelTransform.Value;

		foreach (BoneReference bone in this.bones)
		{
			bone.SetModelSpaceTransform(modelSpaceTransform);
		}
	}

	public void SetLocalTransform(Transform localTransform)
	{
		foreach (BoneReference bone in this.bones)
		{
			bone.SetLocalSpaceTransform(localTransform);
		}
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
		foreach (BoneReference bone in this.bones)
		{
			bone.SetReferenceRelativeTransform(referenceTransform, false);
		}
	}
}
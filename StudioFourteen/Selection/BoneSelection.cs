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

namespace StudioFourteen.Selection;

using Dalamud.Plugin.Services;
using FontAwesome.Sharp;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.Posing;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

using StudioTransform = StudioFourteen.Transform;

public class BoneSelectionId(string boneName, int objectTableIndex)
	: IAsyncSelectionId
{
	public string BoneName { get; init; } = boneName;
	public int ObjectTableIndex { get; init; } = objectTableIndex;

	public override async Task<SelectionBase?> CreateAsync()
	{
		await Threads.FrameworkThread();
		return ServiceManager.Instance.Pose.FindBone(this.ObjectTableIndex, this.BoneName);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.BoneName, this.ObjectTableIndex);
	}
}

public class BoneSelection : TransformSelectionBase
{
	// Bones default to rotation, so just set any translation or scale modes here.
	private static readonly Dictionary<string, TransformHandleTypes> DefaultBoneGizmos = new()
	{
		// Face
		{ "j_f_mmayu_l", TransformHandleTypes.Translation },
		{ "j_f_mayu_l", TransformHandleTypes.Translation },
		{ "j_f_miken_01_l", TransformHandleTypes.Translation },
		{ "j_f_miken_02_l", TransformHandleTypes.Translation },
		{ "j_f_dmiken_02_l", TransformHandleTypes.Translation },
		{ "j_f_uhana", TransformHandleTypes.Translation },
		{ "j_f_hana_l", TransformHandleTypes.Translation },
		{ "j_f_dmemoto_l", TransformHandleTypes.Translation },
		{ "j_f_hoho_l", TransformHandleTypes.Translation },
		{ "j_f_dhoho_l", TransformHandleTypes.Translation },
		{ "j_f_shoho_l", TransformHandleTypes.Translation },

		// Mouth
		{ "j_f_ulip_01_l", TransformHandleTypes.Translation },
		{ "j_f_ulip_02_l", TransformHandleTypes.Translation },
		{ "j_f_umlip_01_l", TransformHandleTypes.Translation },
		{ "j_f_umlip_02_l", TransformHandleTypes.Translation },
		{ "j_f_uslip_l", TransformHandleTypes.Translation },
		{ "j_f_dlip_01_l", TransformHandleTypes.Translation },
		{ "j_f_dlip_02_l", TransformHandleTypes.Translation },
		{ "j_f_dmlip_01_l", TransformHandleTypes.Translation },
		{ "j_f_dmlip_02_l", TransformHandleTypes.Translation },
		{ "j_f_dslip_l", TransformHandleTypes.Translation },

		// Eyes
		{ "j_f_mabup_03in_l", TransformHandleTypes.Translation },
		{ "j_f_mabup_02out_l", TransformHandleTypes.Translation },
		{ "j_f_mabdn_03in_l", TransformHandleTypes.Translation },
		{ "j_f_mabdn_02out_l", TransformHandleTypes.Translation },
	};

	private static readonly Dictionary<string, MirrorModes> DefaultMirrorModes = new()
	{
		{ "j_f_eye_l", MirrorModes.MirrorTCopyRS },
	};

	private readonly List<BoneReference> bones = new();
	private BoneReference? bone;

	public BoneSelection(Dictionary<BoneId, List<BoneId>> bonePaths, string name)
	{
		this.BoneName = name;
		this.BonePaths = bonePaths;

		this.IsFaceBone = name.StartsWith("j_f_");

		this.Name = Resources.Find($"LOC_Bone_{this.BoneName}", this.BoneName);
		this.Subtitle = name;
		this.Description = Resources.Find($"LOC_Bone_{this.BoneName}_Tooltip", string.Empty);
	}

	public Dictionary<BoneId, List<BoneId>> BonePaths { get; private set; }
	public override IconChar Icon => IconChar.Bone;
	public override string TypeName => Resources.Find("LOC_Selection_Bone", "Bone");
	public string BoneName { get; init; }

	public bool IsFaceBone { get; private set; }
	public override double TranslationLargeChange => this.IsFaceBone ? 0.01 : 0.1;
	public override double TranslationSmallChange => this.IsFaceBone ? 0.001 : 0.01;
	public override double TranslationRange => this.IsFaceBone ? 0.02 : 0.1;
	public override int DecimalPlacesToDisplay => this.IsFaceBone ? 4 : 2;
	public override bool CanReset => true;
	public override double GizmoSensitivity => this.IsFaceBone ? 0.05 : 0.5;

	public override TransformHandleTypes DefaultGizmo
	{
		get
		{
			TransformHandleTypes gizmo;
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

			return TransformHandleTypes.Rotation;
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
			foreach (BoneReference bone in this.bones)
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

	public override ISelectionId Id => new BoneSelectionId(this.BoneName, this.BonePaths.Keys.First().ObjectTableIndex);

	public override void Activate()
	{
		this.bones.Clear();
		foreach (BoneId boneId in this.BonePaths.Keys)
		{
			this.bones.Add(ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId));
		}

		this.bone = this.bones[0];
		this.RaisePropertyChanged(nameof(this.IsReady));

		this.MirrorMode = this.GetDefaultMirrorMode();
	}

	public override void Deactivate()
	{
		this.bones.Clear();
		this.bone = null;
	}

	public override void Reset()
	{
		foreach (BoneReference bone in this.bones)
		{
			if (this.MirrorMode != MirrorModes.None && bone.Mirror != null)
			{
				bone.Mirror.Reset(false);
			}

			bone.Reset(false);
		}
	}

	public override bool Equals(SelectionBase? other)
	{
		if (other is not BoneSelection otherBone)
			return false;

		if (this.BonePaths.Count != otherBone.BonePaths.Count)
			return false;

		foreach (BoneId id in this.BonePaths.Keys)
		{
			if (!otherBone.BonePaths.ContainsKey(id))
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

		Transform? modelSpaceTransform;
		bool success = Transform.Divide(transform, this.bone.ModelTransform.Value, out modelSpaceTransform);

		if (success && modelSpaceTransform != null)
		{
			foreach (BoneReference bone in this.bones)
			{
				bone.SetModelSpaceTransform((Transform)modelSpaceTransform);
			}
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

	public MirrorModes GetDefaultMirrorMode()
	{
		MirrorModes mirror = MirrorModes.None;
		if (DefaultMirrorModes.TryGetValue(this.BoneName, out mirror))
		{
			return mirror;
		}

		string? mirrorName = PoseService.GetMirrorBoneName(this.BoneName);
		if (mirrorName != null)
		{
			if (DefaultMirrorModes.TryGetValue(mirrorName, out mirror))
			{
				return mirror;
			}
		}

		return mirror;
	}

	public override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		this.IsReady =
			this.bone != null
			&& this.bone.LocalSpaceTransform != null
			&& this.bone.ReferenceRelativeTransform != null;
	}
}
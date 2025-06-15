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

using StudioFourteen.Scene;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

using StudioTransform = StudioFourteen.Transform;

public class BoneSceneObject : TransformSceneObjectBase
{
	private static readonly Dictionary<string, MirrorModes> DefaultMirrorModes = new()
	{
		{ "j_f_eye_l", MirrorModes.MirrorTCopyRS },
	};

	private readonly List<BoneReference> bones = new();
	private BoneReference? bone;
	private bool isReading = false;

	public BoneSceneObject(Dictionary<BoneId, List<BoneId>> bonePaths, string name)
	{
		this.BoneName = name;
		this.BonePaths = bonePaths;

		this.IsFaceBone = name.StartsWith("j_f_");

		this.Name = Resources.Find($"LOC_Bone_{this.BoneName}", this.BoneName);
		this.Subtitle = name;
		this.Description = Resources.Find($"LOC_Bone_{this.BoneName}_Tooltip", string.Empty);
	}

	public Dictionary<BoneId, List<BoneId>> BonePaths { get; private set; }

	public override string Id => new($"Bone:{this.BoneName}:{this.BonePaths.Keys.First().ObjectTableIndex}");
	public override object? Icon => Resources.Find("ICON_Selection_Bone");
	public override string TypeName => Resources.Find("LOC_Selection_Bone", "Bone");

	public string BoneName { get; init; }

	public bool IsFaceBone { get; private set; }
	public override double TranslationChange => this.IsFaceBone ? 0.01 : 0.1;
	public override int DecimalPlacesToDisplay => this.IsFaceBone ? 4 : 2;
	public override double GizmoSensitivity => this.IsFaceBone ? 0.05 : 0.5;

	public MirrorModes MirrorMode
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

	public Transform ReferenceRelativeTransform
	{
		get => this.bone?.ReferenceRelativeTransform ?? default;
		set => this.SetReferenceTransform(value);
	}

	public void Activate()
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

	public override void OnGameTick()
	{
		base.OnGameTick();

		this.IsReady =
			this.bone != null
			&& this.bone.LocalSpaceTransform != null
			&& this.bone.ReferenceRelativeTransform != null;

		if (this.bone == null
			|| this.bone.LocalSpaceTransform == null
			|| this.bone.ReferenceRelativeTransform == null
			|| this.bone.ModelSpaceTransform == null
			|| this.bone.ModelTransform == null)
			return;

		this.LockTransform = this.bone.Locked;

		this.isReading = true;
		this.WorldTransform = this.bone.ModelSpaceTransform.Value * this.bone.ModelTransform.Value;
		this.LocalTransform = (Transform)this.bone.LocalSpaceTransform;
		this.isReading = false;
	}

	protected override void OnLockTransformChanged(bool oldValue, bool newValue)
	{
		base.OnLockTransformChanged(oldValue, newValue);

		if (this.bone == null)
			return;

		this.bone.Locked = newValue;
	}

	protected override void OnWorldTransformChanged(StudioTransform oldValue, StudioTransform newValue)
	{
		base.OnWorldTransformChanged(oldValue, newValue);

		if (this.isReading || this.bone == null || this.bone.ModelTransform == null)
			return;

		Transform? modelSpaceTransform;
		bool success = Transform.Divide(newValue, this.bone.ModelTransform.Value, out modelSpaceTransform);

		if (success && modelSpaceTransform != null)
		{
			foreach (BoneReference bone in this.bones)
			{
				bone.SetModelSpaceTransform((Transform)modelSpaceTransform);
			}
		}
	}

	protected override void OnLocalTransformChanged(StudioTransform oldValue, StudioTransform newValue)
	{
		base.OnLocalTransformChanged(oldValue, newValue);

		if (this.isReading)
			return;

		Quaternion from = oldValue.Rotation;
		Quaternion to = newValue.Rotation;

		foreach (BoneReference bone in this.bones)
		{
			bone.SetLocalSpaceTransform(newValue);
		}
	}
}
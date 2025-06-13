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

using FontAwesome.Sharp;
using StudioFourteen.Plugin;
using StudioFourteen.Scene;
using StudioFourteen.Selection;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfUtils.Extensions;

public class BlendSelection : SceneObjectBase
{
	private readonly int objectTableIndex;
	private readonly string blendTargetName;
	private readonly List<BoneBlend> bones = new();
	private double value;
	private MirrorModes mirrorMode;

	public BlendSelection(string blendTargetName, BlendTarget target, int objectTableIndex)
	{
		this.objectTableIndex = objectTableIndex;
		this.blendTargetName = blendTargetName;
		this.Name = blendTargetName;
		this.Target = target;
		this.mirrorMode = target.MirrorMode;
	}

	public override string Id => $"Blend:{this.blendTargetName}:{this.objectTableIndex}";
	public override string TypeName => Resources.Find("LOC_Selection_Blend", "Blend");
	public override object? Icon => Resources.Find("ICON_Selection_Blend");

	public BlendTarget Target { get; private set; }
	public bool Flip { get; set; }

	public double Value
	{
		get => this.value;
		set => this.SetValue(value);
	}

	public bool HasLeft
	{
		get
		{
			if (this.bones.Count <= 0)
				return false;

			return this.bones[0].Left != null;
		}
	}

	public MirrorModes MirrorMode
	{
		get => this.mirrorMode;
		set
		{
			this.mirrorMode = value;

			foreach (BoneBlend bone in this.bones)
			{
				bone.Selection.MirrorMode = value;
			}
		}
	}

	public void SetValue(double value)
	{
		this.value = value;

		foreach (BoneBlend bone in this.bones)
		{
			bone.Blend((float)value);
		}
	}

	public override void Activate()
	{
		base.Activate();
		this.Initialize(this.Flip).Run();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	public async Task Initialize(bool flipSides)
	{
		await TickService.GameTick();

		List<Bone>? boneSelections = this.Target.GetBones(this.objectTableIndex, flipSides);
		if (boneSelections == null)
			return;

		foreach (Bone selection in boneSelections)
		{
			selection.Activate();
		}

		await Task.Delay(50);
		await TickService.GameTick();

		foreach (Bone boneSelection in boneSelections)
		{
			if (boneSelection.BoneName == null)
				continue;

			string boneName = boneSelection.BoneName;
			if (flipSides)
				boneName = PoseService.GetMirrorBoneName(boneName) ?? boneName;

			boneSelection.MirrorMode = this.mirrorMode;

			Transform? fromTransform = boneSelection.ReferenceRelativeTransform;
			if (fromTransform == null)
				continue;

			BoneTransform? rightTransform = null;
			this.Target.RightBones?.TryGetValue(boneName, out rightTransform);
			if (rightTransform == null)
				continue;

			if (flipSides)
				rightTransform = FlipUtility.Flip(rightTransform);

			BoneTransform? leftTransform = null;
			this.Target.LeftBones?.TryGetValue(boneName, out leftTransform);
			if (flipSides && leftTransform != null)
				leftTransform = FlipUtility.Flip(leftTransform);

			this.bones.Add(new(boneSelection, (Transform)fromTransform, rightTransform, leftTransform));
		}
	}

	public override void Reset()
	{
		base.Reset();

		foreach (BoneBlend bone in this.bones)
		{
			bone.Selection.Reset();
		}
	}

	public struct BoneBlend(Bone selection, Transform initial, BoneTransform right, BoneTransform? left = null)
	{
		public BoneTransform Value = new();

		public Bone Selection = selection;
		public Transform Initial = initial;
		public BoneTransform Right = right;
		public BoneTransform? Left = left;

		public void Blend(float value)
		{
			if (!this.Initial.ToTRS(out Vector3 initialTranslation, out Quaternion initialRotation, out Vector3 initialScale))
				return;

			if (value == 1)
			{
				this.Selection.SetReferenceTransform(this.Right);
			}
			else if (value > 0)
			{
				if (this.Right.Translation != null)
					this.Value.Translation = Vector3.Lerp(initialTranslation, this.Right.Translation.Value, value);

				if (this.Right.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(initialRotation, this.Right.Rotation.Value, value);

				if (this.Right.Scale != null)
					this.Value.Scale = Vector3.Lerp(initialScale, this.Right.Scale.Value, value);
			}
			else if (value < 0 && this.Left != null)
			{
				if (this.Left.Translation != null)
					this.Value.Translation = Vector3.Lerp(initialTranslation, this.Left.Translation.Value, -value);

				if (this.Left.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(initialRotation, this.Left.Rotation.Value, -value);

				if (this.Left.Scale != null)
					this.Value.Scale = Vector3.Lerp(initialScale, this.Left.Scale.Value, -value);
			}
			else
			{
				this.Value.Translation = initialTranslation;
				this.Value.Rotation = initialRotation;
				this.Value.Scale = initialScale;
			}

			this.Selection.SetReferenceTransform(this.Value);
		}
	}
}

[Serializable]
public class BlendTarget
{
	public string? IconPath { get; set; }
	public MirrorModes MirrorMode { get; set; }
	public Dictionary<string, BoneTransform>? RightBones { get; set; }
	public Dictionary<string, BoneTransform>? LeftBones { get; set; }

	public BitmapSource? Icon
	{
		get
		{
			if (string.IsNullOrEmpty(this.IconPath))
				return null;

			try
			{
				BitmapImage bmp = new();
				bmp.BeginInit();
				bmp.UriSource = new($"pack://application:,,,/StudioFourteen;component/{this.IconPath}");
				bmp.EndInit();
				return bmp;
			}
			catch (Exception ex)
			{
				Logging.Shared.Error(ex, "Failed to load Blend Target Icon");
			}

			return null;
		}
	}

	public List<Bone>? GetBones(int objectTableIndex, bool flipBones)
	{
		TickService.VerifyGameTickThread();

		List<Bone> selections = new();

		if (this.RightBones == null)
			return null;

		foreach ((string boneName, BoneTransform transform) in this.RightBones)
		{
			string getBoneName = boneName;
			if (flipBones)
				getBoneName = PoseService.GetMirrorBoneName(boneName) ?? boneName;

			Bone? selection = ServiceManager.Instance.Pose.FindBone(objectTableIndex, getBoneName);

			if (selection == null)
				continue;

			selections.Add(selection);
		}

		return selections;
	}
}
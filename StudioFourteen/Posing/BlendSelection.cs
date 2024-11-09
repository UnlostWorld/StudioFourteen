namespace StudioFourteen.Posing;
using StudioFourteen.Files;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfUtils.Extensions;

public class BlendSelection(string name, BlendTarget target, int objectTableIndex)
	: SelectionBase
{
	private readonly int objectTableIndex = objectTableIndex;
	private readonly List<BoneBlend> bones = new();
	private double value;
	private MirrorModes mirrorMode = target.MirrorMode;

	public override string Name => name;
	public override string? Subtitle => null;
	public BlendTarget Target => target;
	public override bool CanMirror => true;
	public double Maximum => 1.0;
	public double Minimum => this.HasLeft ? -1.0 : 0.0;
	public override bool CanReset => true;

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

	public override MirrorModes MirrorMode
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
		await Threads.FrameworkThread();

		List<BoneSelection>? boneSelections = this.Target.GetBones(this.objectTableIndex, flipSides);
		if (boneSelections == null)
			return;

		foreach(BoneSelection selection in boneSelections)
		{
			selection.Activate();
		}

		await Threads.NextFrame();

		foreach (BoneSelection boneSelection in boneSelections)
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
				rightTransform = rightTransform.Flip();

			BoneTransform? leftTransform = null;
			this.Target.LeftBones?.TryGetValue(boneName, out leftTransform);
			if (flipSides && leftTransform != null)
				leftTransform = leftTransform.Flip();

			this.bones.Add(new(boneSelection, (Transform)fromTransform, rightTransform, leftTransform));
		}
	}

	public override void Reset()
	{
		base.Reset();

		foreach(BoneBlend bone in this.bones)
		{
			bone.Selection.Reset();
		}
	}

	public struct BoneBlend(BoneSelection selection, Transform initial, BoneTransform right, BoneTransform? left = null)
	{
		public BoneTransform Value = new();

		public BoneSelection Selection = selection;
		public Transform Initial = initial;
		public BoneTransform Right = right;
		public BoneTransform? Left = left;

		public void Blend(float value)
		{
			if (value > 0)
			{
				if (this.Right.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(this.Initial.Rotation, this.Right.Rotation.Value, value);

				if (this.Right.Translation != null)
					this.Value.Translation = Vector3.Lerp(this.Initial.Translation, this.Right.Translation.Value, value);

				if (this.Right.Scale != null)
					this.Value.Scale = Vector3.Lerp(this.Initial.Scale, this.Right.Scale.Value, value);
			}
			else if (value < 0 && this.Left != null)
			{
				if (this.Left.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(this.Initial.Rotation, this.Left.Rotation.Value, -value);

				if (this.Left.Translation != null)
					this.Value.Translation = Vector3.Lerp(this.Initial.Translation, this.Left.Translation.Value, -value);

				if (this.Left.Scale != null)
					this.Value.Scale = Vector3.Lerp(this.Initial.Scale, this.Left.Scale.Value, -value);
			}
			else
			{
				this.Value.Translation = this.Initial.Translation;
				this.Value.Rotation = this.Initial.Rotation;
				this.Value.Scale = this.Initial.Scale;
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

	public List<BoneSelection>? GetBones(int objectTableIndex, bool flipBones)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		List<BoneSelection> selections = new();

		if (this.RightBones == null)
			return null;

		foreach ((string boneName, BoneTransform transform) in this.RightBones)
		{
			string getBoneName = boneName;
			if (flipBones)
				getBoneName = PoseService.GetMirrorBoneName(boneName) ?? boneName;

			BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(objectTableIndex, getBoneName);

			if (selection == null)
				continue;

			selections.Add(selection);
		}

		return selections;
	}
}
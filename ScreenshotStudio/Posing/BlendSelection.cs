namespace ScreenshotStudio.Posing;
using ScreenshotStudio.Files;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using WpfUtils.Extensions;
using static ScreenshotStudio.Files.PoseFile;

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
		this.Initialize().Run();
	}

	public override void Deactivate()
	{
		base.Deactivate();
	}

	public async Task Initialize()
	{
		await Threads.FrameworkThread();

		List<BoneSelection>? boneSelections = this.Target.GetBones(this.objectTableIndex);
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

			boneSelection.MirrorMode = this.mirrorMode;

			BoneTransform? fromTransform = boneSelection.GetLiveReferenceRelativeTransform();
			if (fromTransform == null)
				continue;

			BoneTransform? rightTransform = null;
			this.Target.RightBones?.TryGetValue(boneSelection.BoneName, out rightTransform);

			if (rightTransform == null)
				continue;

			BoneTransform? leftTransform = null;
			this.Target.LeftBones?.TryGetValue(boneSelection.BoneName, out leftTransform);

			this.bones.Add(new(boneSelection, fromTransform, rightTransform, leftTransform));
		}
	}

	public struct BoneBlend(BoneSelection selection, BoneTransform initial, BoneTransform right, BoneTransform? left = null)
	{
		public BoneTransform Value = new();

		public BoneSelection Selection = selection;
		public BoneTransform Initial = initial;
		public BoneTransform Right = right;
		public BoneTransform? Left = left;

		public void Blend(float value)
		{
			if (value > 0)
			{
				if (this.Initial.Rotation != null && this.Right.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(this.Initial.Rotation.Value, this.Right.Rotation.Value, value);

				if (this.Initial.Translation != null && this.Right.Translation != null)
					this.Value.Translation = Vector3.Lerp(this.Initial.Translation.Value, this.Right.Translation.Value, value);

				if (this.Initial.Scale != null && this.Right.Scale != null)
					this.Value.Scale = Vector3.Lerp(this.Initial.Scale.Value, this.Right.Scale.Value, value);
			}
			else if (value < 0 && this.Left != null)
			{
				if (this.Initial.Rotation != null && this.Left.Rotation != null)
					this.Value.Rotation = Quaternion.Lerp(this.Initial.Rotation.Value, this.Left.Rotation.Value, -value);

				if (this.Initial.Translation != null && this.Left.Translation != null)
					this.Value.Translation = Vector3.Lerp(this.Initial.Translation.Value, this.Left.Translation.Value, -value);

				if (this.Initial.Scale != null && this.Left.Scale != null)
					this.Value.Scale = Vector3.Lerp(this.Initial.Scale.Value, this.Left.Scale.Value, -value);
			}
			else
			{
				if (this.Initial.Rotation != null)
					this.Value.Rotation = this.Initial.Rotation.Value;

				if (this.Initial.Translation != null)
					this.Value.Translation = this.Initial.Translation.Value;

				if (this.Initial.Scale != null)
					this.Value.Scale = this.Initial.Scale.Value;
			}

			this.Selection.ApplyReferenceTransform(this.Value);
		}
	}
}

[System.Serializable]
public class BlendTarget
{
	public string? IconPath { get; set; }
	public MirrorModes MirrorMode { get; set; }
	public Dictionary<string, PoseFile.BoneTransform>? RightBones { get; set; }
	public Dictionary<string, PoseFile.BoneTransform>? LeftBones { get; set; }

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
				bmp.UriSource = new($"pack://application:,,,/ScreenshotStudio;component/{this.IconPath}");
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

	public List<BoneSelection>? GetBones(int objectTableIndex)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		List<BoneSelection> selections = new();

		if (this.RightBones == null)
			return null;

		foreach ((string boneName, BoneTransform transform) in this.RightBones)
		{
			BoneSelection? selection = ServiceManager.Instance.Pose.FindBone(objectTableIndex, boneName);

			if (selection == null)
				continue;

			selections.Add(selection);
		}

		return selections;
	}
}
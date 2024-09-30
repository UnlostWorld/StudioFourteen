namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Files;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using static ScreenshotStudio.Files.PoseFile;

public class BlendService : ServiceBase
{
	public async Task<Blend?> BeginBlend(int objectTableIndex, BlendTarget toPose, MirrorModes mirrorMode)
	{
		await Threads.FrameworkThread();

		List<BoneSelection>? boneSelections = toPose.GetBones(objectTableIndex);
		if (boneSelections == null)
			return null;

		await Threads.NextFrame();

		List<BoneBlend> bones = new();
		foreach (BoneSelection boneSelection in boneSelections)
		{
			if (boneSelection.Name == null)
				continue;

			BoneTransform? fromTransform = boneSelection.GetLiveReferenceRelativeTransform();
			if (fromTransform == null)
				continue;

			BoneTransform? rightTransform = null;
			toPose.RightBones?.TryGetValue(boneSelection.Name, out rightTransform);

			if (rightTransform == null)
				continue;

			BoneTransform? leftTransform = null;
			toPose.LeftBones?.TryGetValue(boneSelection.Name, out leftTransform);

			bones.Add(new(boneSelection, fromTransform, rightTransform, leftTransform));
		}

		Blend blend = new(bones);
		blend.MirrorMode = mirrorMode;
		blend.Value = 0;

		return blend;
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

	public class Blend(List<BoneBlend> bones)
	{
		private readonly List<BoneBlend> bones = bones;
		private double value;

		public double Maximum => 1.0;
		public double Minimum => this.HasLeft ? -1.0 : 0.0;

		public bool FlipSides { get; set; }

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
			get
			{
				if (this.bones.Count <= 0)
					return MirrorModes.None;

				return this.bones[0].Selection.MirrorMode;
			}

			set
			{
				foreach(BoneBlend bone in this.bones)
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
	}
}

public class BlendSelection(string name, BlendTarget target)
	: SelectionBase
{
	private MirrorModes mirrorMode = target.MirrorMode;

	public override string Name => name;
	public override string? Subtitle => null;

	public BlendTarget Target => target;

	public override bool CanMirror => true;
	public override MirrorModes MirrorMode
	{
		get => this.Blend?.MirrorMode ?? this.mirrorMode;
		set
		{
			this.mirrorMode = value;

			if (this.Blend == null)
				return;

			this.Blend.MirrorMode = value;
		}
	}

	public BlendService.Blend? Blend { get; set; }
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
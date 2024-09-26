namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Lua;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Files;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
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
	public async Task<Blend?> BeginBlend(int objectTableIndex, BlendTarget toPose)
	{
		await Threads.FrameworkThread();

		List<BoneReference>? boneReferences = toPose.GetBoneReferences(objectTableIndex);
		if (boneReferences == null)
			return null;
		await Threads.NextFrame();

		List<BoneBlend> bones = new();
		foreach (BoneReference boneReference in boneReferences)
		{
			if (boneReference.Name == null)
				continue;

			BoneTransform? fromTransform = boneReference.GetLiveReferenceRelativeTransform();
			if (fromTransform == null)
				continue;

			BoneTransform? rightTransform = null;
			toPose.RightBones?.TryGetValue(boneReference.Name, out rightTransform);

			if (rightTransform == null)
				continue;

			BoneTransform? leftTransform = null;
			toPose.LeftBones?.TryGetValue(boneReference.Name, out leftTransform);

			bones.Add(new(boneReference, fromTransform, rightTransform, leftTransform));
		}

		Blend blend = new(bones);
		blend.Value = 0;

		return blend;
	}

	public struct BoneBlend(BoneReference reference, BoneTransform initial, BoneTransform right, BoneTransform? left = null)
	{
		public BoneTransform Value = new();

		public BoneReference Reference = reference;
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

			this.Reference.LoadRelativeTransform = this.Value;
		}
	}

	public class Blend(List<BoneBlend> bones)
	{
		private readonly List<BoneBlend> bones = bones;
		private double value;

		public double Maximum => 1.0;
		public double Minimum => this.HasLeft ? -1.0 : 0.0;

		public double Value
		{
			get => this.value;
			set => this.SetValue(value);
		}

		public bool HasLeft => this.bones[0].Left != null;

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
	public override string Name => name;
	public override string? Subtitle => null;

	public BlendTarget Target => target;
}

[System.Serializable]
public class BlendTarget
{
	public string? IconPath { get; set; }
	public Dictionary<string, PoseFile.BoneTransform>? RightBones { get; set; }
	public Dictionary<string, PoseFile.BoneTransform>? LeftBones { get; set; }

	public BitmapSource? Icon
	{
		get
		{
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

	public List<BoneReference>? GetBoneReferences(int objectTableIndex)
	{
		Threads.VerifyFrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return null;

		List<BoneReference> boneReferences = new();

		unsafe
		{
			Character* character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			if (character == null)
				return null;

			CharacterBase* characterBase = character->GetCharacterBase();
			if (characterBase == null)
				return null;

			ushort partialCount = characterBase->Skeleton->PartialSkeletonCount;
			for (int partialIdx = 0; partialIdx < partialCount; partialIdx++)
			{
				PartialSkeleton* partialSkeleton = &characterBase->Skeleton->PartialSkeletons[partialIdx];

				byte poseCount = partialSkeleton->GetMaxPoses();
				for (byte poseIdx = 0; poseIdx < poseCount; poseIdx++)
				{
					hkaPose* pose = partialSkeleton->GetHavokPose(poseIdx);
					if (pose == null)
						continue;

					int boneCount = pose->Skeleton->Bones.Length;
					for (short boneIdx = 0; boneIdx < boneCount; boneIdx++)
					{
						hkaBone bone = pose->Skeleton->Bones[boneIdx];
						string? boneName = bone.Name.String;

						if (boneName == null)
							continue;

						bool include = false;

						if (this.RightBones != null)
							include |= this.RightBones.ContainsKey(boneName);

						if (this.LeftBones != null)
							include |= this.LeftBones.ContainsKey(boneName);

						if (include)
						{
							BoneId boneId = new(character->ObjectIndex, partialIdx, poseIdx, boneIdx);
							BoneReference reference = ServiceManager.Instance.Pose.GetOrCreateBoneReference(boneId, boneName);
							boneReferences.Add(reference);
						}
					}
				}
			}
		}

		return boneReferences;
	}
}
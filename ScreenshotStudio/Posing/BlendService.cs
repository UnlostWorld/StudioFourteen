namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using ScreenshotStudio.Files;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
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

			BoneTransform? val = null;
			toPose.Bones?.TryGetValue(boneReference.Name, out val);

			if (val == null)
				continue;

			BoneTransform? fromTransform = boneReference.GetLiveReferenceRelativeTransform();
			if (fromTransform == null)
				continue;

			bones.Add(new(boneReference, fromTransform, val));
		}

		Blend blend = new(bones);
		blend.Value = 0;

		return blend;
	}

	public struct BoneBlend(BoneReference reference, BoneTransform from, BoneTransform to)
	{
		public BoneTransform Value = new();

		public BoneReference Reference = reference;
		public BoneTransform From = from;
		public BoneTransform To = to;

		public void Blend(float value)
		{
			if (this.From.Rotation != null && this.To.Rotation != null)
				this.Value.Rotation = Quaternion.Lerp(this.From.Rotation.Value, this.To.Rotation.Value, value);

			if (this.From.Translation != null && this.To.Translation != null)
				this.Value.Translation = Vector3.Lerp(this.From.Translation.Value, this.To.Translation.Value, value);

			if (this.From.Scale != null && this.To.Scale != null)
				this.Value.Scale = Vector3.Lerp(this.From.Scale.Value, this.To.Scale.Value, value);

			this.Reference.LoadRelativeTransform = this.Value;
		}
	}

	public class Blend(List<BoneBlend> bones)
	{
		private readonly List<BoneBlend> bones = bones;
		private double value;

		public double Value
		{
			get => this.value;
			set => this.SetValue(value);
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

public class BlendSelection(string name, List<BlendTarget> targets)
	: SelectionBase
{
	public override string Name => name;
	public override string? Subtitle => null;
	public List<BlendTarget> Targets => targets;
}

[System.Serializable]
public class BlendTarget
{
	public string? IconPath { get; set; }
	public Dictionary<string, PoseFile.BoneTransform>? Bones { get; set; }

	public BitmapSource Icon
	{
		get
		{
			BitmapImage bmp = new();
			bmp.BeginInit();
			bmp.UriSource = new($"pack://application:,,,/ScreenshotStudio;component/{this.IconPath}");
			bmp.EndInit();
			return bmp;
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

						BoneTransform? val = null;
						this.Bones?.TryGetValue(boneName, out val);

						if (val != null)
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
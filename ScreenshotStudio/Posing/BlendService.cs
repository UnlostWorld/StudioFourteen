namespace ScreenshotStudio.Posing;

using ScreenshotStudio.Files;
using ScreenshotStudio.Library;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;

using static ScreenshotStudio.Files.PoseFile;

public class BlendService : ServiceBase
{
	public BlendFileSource? MouthSource { get; set; }

	public override Task Start()
	{
		this.MouthSource = new("Mouth");
		this.MouthSource.ScanSource();

		return base.Start();
	}

	public async Task<Blend?> BeginBlend(int objectTableIndex, PoseFile toPose)
	{
		await Threads.FrameworkThread();

		List<BoneReference>? boneReferences = toPose.GetBoneReferences(objectTableIndex, true);
		if (boneReferences == null)
			return null;

		await Threads.NextFrame();

		List<BoneBlend> bones = new();
		foreach (BoneReference boneReference in boneReferences)
		{
			if (boneReference.Name == null)
				continue;

			BoneTransform? val = null;
			toPose.ReferenceRelativeBones?.TryGetValue(boneReference.Name, out val);

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

public class BlendFileSource : SourceBase
{
	public FileEntry? SmileBlend;

	private readonly DirectoryInfo? directoryInfo;
	private readonly FileTypeInfoBase? fileTypeInfo;

	public BlendFileSource(string dirName)
	{
		this.DisplayName = dirName;
		this.fileTypeInfo = this.Services.Files.GetTypeInfo<PoseFile>();

		DirectoryInfo? dir = DalamudServices.PluginInterface?.AssemblyLocation.Directory;
		if (dir == null)
			return;

		this.directoryInfo = new DirectoryInfo($"{dir.FullName}/Assets/ExpressionBlends/{dirName}/");
	}

	public string DisplayName { get; init; }
	public override string Name => "Blend Targets File Source";
	protected override string GetInternalId() => "BlendTargets";

	protected override void Scan()
	{
		if (this.directoryInfo == null || this.fileTypeInfo == null)
			return;

		FileInfo[] files = this.directoryInfo.GetFiles($"*{this.fileTypeInfo.Extension}");

		foreach (FileInfo file in files)
		{
			this.Add(new FileEntry(this, file, this.fileTypeInfo));
		}
	}
}

public class BlendSelection(BlendFileSource fileSource)
	: SelectionBase
{
	public override string Name => fileSource.DisplayName;
	public override string? Subtitle => null;

	public List<FileEntry> BlendTargets
	{
		get
		{
			List<FileEntry> result = new();
			if (fileSource.AllEntries != null)
			{
				foreach (ILibraryEntry entry in fileSource.AllEntries)
				{
					if (entry is FileEntry file)
					{
						result.Add(file);
					}
				}
			}

			return result;
		}
	}
}
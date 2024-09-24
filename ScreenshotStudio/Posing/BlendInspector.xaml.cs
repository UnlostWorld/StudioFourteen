namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Files;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Structs;
using ScreenshotStudio.Structs.Extensions;
using ScreenshotStudio.Utilities;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using static ScreenshotStudio.Files.PoseFile;

[DependencyProperty<BlendSelection>("Selection")]
public partial class BlendInspector : View
{
	private Blend? activeblend;

	public PoseWindow? Panel => this.FindParent<PoseWindow>();

	[AutoNotify] public FrameworkElement? BlendTargetElement { get; set; }
	[AutoNotify] public bool IsBlendOpen
	{
		get => this.activeblend != null;
		set
		{
			if (this.activeblend == null)
				return;

			this.activeblend.End();
			this.activeblend = null;
		}
	}

	[AutoNotify]
	public double BlendValue
	{
		get => this.activeblend?.Value ?? 0;
		set => this.activeblend?.SetBlend(value);
	}

	private async void OnTargetClicked(object sender, RoutedEventArgs e)
	{
		this.BlendTargetElement = sender as FrameworkElement;

		FileEntry? fileEntry = this.BlendTargetElement?.DataContext as FileEntry;
		if (fileEntry == null)
			return;

		PoseFile? target = fileEntry.File as PoseFile;
		if (target == null)
			return;

		await Threads.FrameworkThread();

		this.activeblend = new(target);
		await this.activeblend.Begin(this.Services.Target.TargetObjectIndex);

		this.Log.Information("Begin!");
	}
}

public class BlendSelection(string name)
	: SelectionBase
{
	public override string Name => name;
	public override string? Subtitle => null;

	public List<FileEntry> BlendTargets { get; init; } = new();
}

public class Blend(PoseFile toPose)
{
	private readonly List<BoneBlend> bones = new();

	public double Value { get; set; }

	public async Task Begin(int objectTableIndex)
	{
		var boneReferences = toPose.GetBoneReferences(objectTableIndex, true);
		if (boneReferences == null)
			return;

		await Threads.NextFrame();

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

			this.bones.Add(new(boneReference, fromTransform, val));
		}

		this.SetBlend(0);
	}

	public void SetBlend(double value)
	{
		this.Value = value;

		foreach(BoneBlend bone in this.bones)
		{
			bone.Blend((float)value);
		}
	}

	public void End()
	{
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
}
namespace ScreenshotStudio.Posing;

using DependencyPropertyGenerator;
using ScreenshotStudio.Files;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Plugin;
using System.IO;
using System.Numerics;
using System.Windows;

[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class ExpressionsView : View
{
	private BlendFileSource? src;

	private BlendSelection? mouthSelection;
	private BlendSelection? leftEyeSelection;
	private BlendSelection? rightEyeSelection;

	public double BackgroundOpacity => SkeletonView.BackgroundOpacity;

	private void OnMouthClicked(object sender, RoutedEventArgs e)
	{
		this.src = new();
		this.src.ScanSource();

		this.Log.Information($">> {this.src.SmileBlend}");

		if (this.src.SmileBlend != null)
			this.mouthSelection?.BlendTargets.Add(this.src.SmileBlend);

		this.Services.Pose.Selection = this.mouthSelection;
	}

	private void OnLeftEyeClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.leftEyeSelection;
	}

	private void OnRightEyeClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = this.rightEyeSelection;
	}

	partial void OnObjectTableIndexChanged(int newValue)
	{
		this.mouthSelection = new("Mouth");
		this.leftEyeSelection = new("Left Eye");
		this.rightEyeSelection = new("Right Eye");
	}

	// Move me
	public class BlendFileSource : SourceBase
	{
		public FileEntry? SmileBlend;

		public override string Name => "Blend Targets File Source";
		protected override string GetInternalId() => "BlendTargets";

		protected override void Scan()
		{
			FileTypeInfoBase? typeInfo = this.Services.Files.GetTypeInfo<PoseFile>();

			if (typeInfo == null)
				return;

			DirectoryInfo? dir = DalamudServices.PluginInterface?.AssemblyLocation.Directory;
			if (dir == null)
				return;

			dir = new DirectoryInfo(dir.FullName + "/Assets/ExpressionBlends/");

			this.SmileBlend = new(
				this,
				new FileInfo(dir.FullName + "Smile.pose"),
				typeInfo);
		}
	}
}
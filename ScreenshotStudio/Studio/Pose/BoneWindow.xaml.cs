// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using ScreenshotStudio.Windows;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class BoneWindow : ActorWindow
{
	public readonly ObservableCollection<BoneView> BoneViews = new();

	private unsafe Skeleton* oldSkeleton;

	public unsafe Skeleton* Skeleton => this.Actor->Model->Skeleton;

	protected override void OnOpened()
	{
		base.OnOpened();
		Task.Run(this.SkeletonWatcher);
	}

	private async Task SkeletonWatcher()
	{
		while(this.IsOpen)
		{
			await Task.Delay(100);

			try
			{
				this.CheckSkeleton();
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error in bone window");
			}
		}
	}

	private unsafe void CheckSkeleton()
	{
		if (this.oldSkeleton != this.Skeleton)
		{
			this.oldSkeleton = this.Skeleton;

			this.Log.Information($"On Skeleton Changed");

			foreach(BoneView boneView in this.BoneViews)
			{
				boneView.OnSkeletonChanged();
			}
		}
	}
}
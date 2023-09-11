// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

public partial class BoneWindow : ActorWindow
{
	public readonly ObservableCollection<BoneView> BoneViews = new();

	private unsafe Skeleton* oldSkeleton;

	public unsafe Skeleton* Skeleton => this.Actor->Model->Skeleton;

	protected PoseWindow? PoseWindow => this.Services.Panels.Get<PoseWindow>();

	public unsafe void Select(string boneName, bool add)
	{
		if (this.PoseWindow == null)
			return;

		BoneReferences? bones = BoneReferences.Search(this.Skeleton, boneName);

		if (bones == null)
			return;

		if (add)
		{
			if (this.PoseWindow.SelectedBones == null)
				this.PoseWindow.SelectedBones = new();

			this.PoseWindow.SelectedBones.Add(bones);
		}
		else
		{
			this.PoseWindow.SelectedBones = bones;
		}
	}

	public unsafe void UnSelect(string boneName)
	{
		if (this.PoseWindow == null)
			return;

		BoneReferences? bones = BoneReferences.Search(this.Skeleton, boneName);

		if (bones == null)
			return;

		this.PoseWindow.SelectedBones?.Remove(bones);
		if (this.PoseWindow.SelectedBones?.Count <= 0)
		{
			this.PoseWindow.SelectedBones = null;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		Task.Run(this.SkeletonWatcher);

		if (this.PoseWindow == null)
			return;

		this.PoseWindow.SelectedBonesChanged += this.OnSelectedBonesChanged;
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		if (this.PoseWindow != null)
		{
			this.PoseWindow.SelectedBonesChanged -= this.OnSelectedBonesChanged;
		}
	}

	private void OnSelectedBonesChanged(BoneReferences? bones)
	{
		foreach (BoneView boneView in this.BoneViews)
		{
			boneView.OnSelectionChanged(bones);
		}
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

			foreach(BoneView boneView in this.BoneViews)
			{
				boneView.OnSkeletonChanged();
			}
		}
	}
}
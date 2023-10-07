namespace ScreenshotStudio.Studio.Pose;

using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok;
using System.Collections.Generic;
using System.Windows.Controls;

public partial class PoseListView : UserControl
{
	public PoseListView()
	{
		this.InitializeComponent();
	}

	public unsafe void OnSkeletonChanged(Skeleton* skeleton)
	{
		this.Dispatcher.Invoke(() => this.GenerateTree(skeleton));
	}

	public unsafe void GenerateTree(Skeleton* skeleton)
	{
		for (int partialSkeletonIndex = 0; partialSkeletonIndex < skeleton->PartialSkeletonCount; partialSkeletonIndex++)
		{
			PartialSkeleton partialSkeleton = skeleton->PartialSkeletons[partialSkeletonIndex];

			TreeViewItem partialItem = new();
			partialItem.Header = $"Partial Skeleton {partialSkeletonIndex}";

			hkaPose* pose = partialSkeleton.GetHavokPose(0);
			if (pose == null)
				continue;

			string? rootBoneName = pose->Skeleton->Bones[0].Name.String;

			// definitelly need a better system for bone localizations than this.
			if (rootBoneName != null)
				rootBoneName = LegacyBoneNameConverter.GetLegacyName(rootBoneName) ?? rootBoneName;
			partialItem.Header = rootBoneName;

			List<ItemsControl> items = new();
			items.Add(partialItem);

			for (int i = 1; i < pose->Skeleton->Bones.Length; i++)
			{
				string? boneName = pose->Skeleton->Bones[i].Name.String;
				int parentIndex = pose->Skeleton->ParentIndices[i];

				// definitelly need a better system for bone localizations than this.
				if (boneName != null)
					boneName = LegacyBoneNameConverter.GetLegacyName(boneName) ?? boneName;

				TreeViewItem item = new();
				item.Header = boneName;
				items.Add(item);

				items[parentIndex].Items.Add(items[i]);
			}

			if (partialItem.Items.Count > 0)
			{
				this.BoneTree.Items.Add(partialItem);
			}
		}
	}
}

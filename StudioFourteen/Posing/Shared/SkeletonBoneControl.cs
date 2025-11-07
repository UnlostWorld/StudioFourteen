// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Posing.Shared;

using System.Collections.Generic;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using StudioFourteen.Scene;

[Logger]
[DependencyProperty<string>("SelectionName")]
[DependencyProperty<string>("Label")]
[DependencyProperty<bool>("IsMouseHover")]
[DependencyProperty<bool>("IsSelected")]
[DependencyProperty<bool>("IsParentSelected")]
[DependencyProperty<bool>("IsValid")]
public partial class SkeletonBoneControl : Control
{
	public SceneObjectBase? Selection { get; set; }
	public string? SafeName { get; private set; }
	public bool IsSafeValid { get; set; }

	public virtual void OnHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		if (newSelection == null || this.Selection == null)
		{
			this.IsMouseHover = false;
			return;
		}

		bool isHover = newSelection.Id == this.Selection.Id;
		this.IsMouseHover = isHover;
	}

	public virtual void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		if (newSelection == null || this.Selection == null)
		{
			this.IsSelected = false;
			return;
		}

		this.IsSelected = newSelection.Id == this.Selection.Id;

		this.IsParentSelected = false;

		// TODO
		/*if (this.Selection is SkeletonBone boneSelection && newSelection is SkeletonBone newBoneSelection)
		{
			foreach((BoneId selectedBoneId, _) in newBoneSelection.BonePaths)
			{
				foreach((BoneId boneId, List<BoneId> pathToRoot) in boneSelection.BonePaths)
				{
					if (pathToRoot.Contains(selectedBoneId))
					{
						this.IsParentSelected = true;
						return;
					}
				}
			}
		}*/
	}

	partial void OnSelectionNameChanged(string? newValue)
	{
		this.SafeName = newValue;
	}
}

public partial class SkeletonBoneGroupControl : SkeletonBoneControl
{
	public List<SkeletonBoneControl>? Bones { get; set; }

	public void UpdatePositions()
	{
		if (this.Bones == null || this.Bones.Count <= 0)
			return;

		double l = double.MaxValue;
		double t = double.MaxValue;
		double r = double.MinValue;
		double b = double.MinValue;

		foreach (SkeletonBoneControl bone in this.Bones)
		{
			double boneL = Canvas.GetLeft(bone);
			double boneT = Canvas.GetTop(bone);
			double boneR = boneL + bone.Width + bone.Margin.Left + bone.Margin.Right;
			double boneB = boneT + bone.Height + bone.Margin.Top + bone.Margin.Bottom;

			if (boneL == double.NaN ||
				boneT == double.NaN ||
				boneR == double.NaN ||
				boneB == double.NaN)
				continue;

			l = double.Min(boneL, l);
			t = double.Min(boneT, t);
			r = double.Max(boneR, r);
			b = double.Max(boneB, b);
		}

		l -= 6;
		t -= 6;
		r += 6;
		b += 6;

		Canvas.SetLeft(this, l);
		Canvas.SetTop(this, t);

		this.Width = r - l;
		this.Height = b - t;
	}

	public override void OnHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Log.Information($">> {newSelection?.Id} == {this.Selection?.Id}");
		base.OnHoverChanged(oldSelection, newSelection, source);
	}
}
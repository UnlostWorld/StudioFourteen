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
using StudioFourteen.Scene;

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

			if (double.IsNaN(boneL) ||
				double.IsNaN(boneT) ||
				double.IsNaN(boneR) ||
				double.IsNaN(boneB))
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
		base.OnHoverChanged(oldSelection, newSelection, source);
	}
}
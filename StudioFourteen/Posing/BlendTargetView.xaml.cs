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

namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using System;
using System.Windows;

[DependencyProperty<string>("TargetName")]
[DependencyProperty<int>("ObjectTableIndex")]
[DependencyProperty<bool>("FlipBones")]
public partial class BlendTargetView : View
{
	[AutoNotify] public BlendTarget? Target { get; private set; }

	partial void OnTargetNameChanged()
	{
		if (this.TargetName == null)
			return;

		throw new NotImplementedException();
	}

	private void OnClick(object sender, RoutedEventArgs e)
	{
		if (this.TargetName == null || this.Target == null)
			return;

		BlendSelection selection = new(this.TargetName, this.Target, this.ObjectTableIndex);
		selection.Flip = this.FlipBones;
		this.Services.Selection.Select(selection, this);
	}
}

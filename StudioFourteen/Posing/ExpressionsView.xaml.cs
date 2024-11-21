// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Posing;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using System.Windows;

[DependencyProperty<int>("ObjectTableIndex", DefaultValue = 1)]
[DependencyProperty<bool>("FlipSides", DefaultValue = false)]
public partial class ExpressionsView : View
{
	public ExpressionsView()
	{
		this.Services.Pose.SelectionChanged += this.OnPoseSelectionChanged;
	}

	public double BackgroundOpacity => SkeletonView.BackgroundOpacity;

	private void OnPoseSelectionChanged(SelectionBase? newSelection)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.MouthToggle.IsChecked = false;
			this.LeftEyeToggle.IsChecked = false;
			this.RightEyeToggle.IsChecked = false;
		});
	}

	partial void OnObjectTableIndexChanged(int newValue)
	{
	}

	private void OnEyeClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Pose.Selection = new EyeSelection(this.ObjectTableIndex);
	}
}
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

using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Serilog;
using StudioFourteen.Utilities;
using WpfUtils;
using WpfUtils.Extensions;

[DependencyProperty<string>("BoneName")]
[DependencyProperty<string>("Label")]
public partial class BoneView : ToggleButton
{
	protected readonly ILogger Log;
	private int objectTableIndex;

	private BoneSelection? boneSelection;

	public BoneView()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		this.Log = Logging.ForContext(this.GetType());
		this.Click += this.OnClicked;
	}

	protected ServiceManager Services => ServiceManager.Instance;

	private void OnTargetChanged(int objectTableIndex)
	{
		this.objectTableIndex = objectTableIndex;
		this.Load().Run();
	}

	partial void OnBoneNameChanged(string? newValue)
	{
		this.Load().Run();
	}

	private async Task Load()
	{
		await this.MainThread();
		string? boneName = this.BoneName;
		await Threads.FrameworkThread();
		if(boneName == null)
			return;

		this.boneSelection = this.Services.Pose.FindBone(this.objectTableIndex, boneName);
		await this.MainThread();
		this.IsEnabled = this.boneSelection != null;
	}

	private void OnClicked(object sender, RoutedEventArgs e)
	{
		if(this.boneSelection == null)
			return;

		this.Services.Selection.Current = this.boneSelection;
	}
}

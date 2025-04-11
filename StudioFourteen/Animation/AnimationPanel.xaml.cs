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

namespace StudioFourteen.Animation;

using System.Windows;
using Lumina.Excel.Sheets;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using WpfUtils.Extensions;

public partial class AnimationPanel : Panel
{
	[Notify] private AnimationService.AnimationController? controller;

	public void OnTestClicked(object sender, RoutedEventArgs args)
	{
		Emote? emote = this.Services.GameData.GetRow<Emote>(121);
		if (emote == null)
			return;

		this.Controller?.PlayEmoteAsync((Emote)emote).Run();
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		this.OnTargetChanged(this.Services.Target.TargetObjectIndex);
	}

	protected override void OnClosed()
	{
		base.OnClosed();
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
	}

	protected void OnTimelinePositionChanged(float oldValue, float newValue)
	{
	}

	private void OnTargetChanged(int objectTableIndex)
	{
		this.Controller = this.Services.Animations.GetController(objectTableIndex);
	}
}
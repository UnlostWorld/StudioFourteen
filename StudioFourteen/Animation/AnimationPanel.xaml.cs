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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Tags;
using WpfUtils.Extensions;

public partial class AnimationPanel : CharacterPanelBase
{
	[Notify] private AnimationService.AnimationController? controller;
	[Notify] private ITimelineAnimation? animation;
	[Notify] private TagCollection defaultTags = new("Player Animation");

	public void OnPlayClicked(object sender, RoutedEventArgs args)
	{
		if (this.animation != null)
		{
			this.Controller?.PlayAnimationAsync(this.animation).Run();
		}
	}

	public void OnStopClicked(object sender, RoutedEventArgs args)
	{
		this.Controller?.ResetAsync().Run();
	}

	protected override void OnTargetChanged(Character? character)
	{
		if (character == null)
		{
			this.controller = null;
		}
		else
		{
			this.Controller = this.Services.Animations.GetController(character.ObjectIndex);
		}
	}
}
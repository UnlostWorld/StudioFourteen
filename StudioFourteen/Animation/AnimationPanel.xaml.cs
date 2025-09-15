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
using StudioFourteen.Panels;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Tags;
using WpfUtils.Extensions;

public partial class AnimationPanel : CharacterPanelBase
{
	public AnimationPanel()
	{
		this.DefaultTags = new("Player Animation");
	}

	[Bind] public partial AnimationController? Controller { get; set; }
	[Bind] public partial ITimelineAnimation? Animation { get; set; }
	[Bind] public partial TagCollection DefaultTags { get; set; }

	public void OnPlayClicked(object sender, RoutedEventArgs args)
	{
		if (this.Animation != null)
		{
			this.Controller?.PlayAnimationAsync(this.Animation).Run();
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
			this.Controller = null;
		}
		else
		{
			this.Controller = this.Services.Animations.GetController(character.ObjectIndex);
		}
	}
}
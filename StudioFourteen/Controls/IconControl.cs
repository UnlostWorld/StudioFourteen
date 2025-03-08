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

namespace StudioFourteen.Controls;

using System;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;

[DependencyProperty<string>("Icon")]
public partial class IconControl : Control
{
	[Notify] private object? content;

	partial void OnIconChanged(string? newValue)
	{
		if (newValue == null)
			return;

		if (newValue.StartsWith("fa-"))
		{
			try
			{
				this.Content = Enum.Parse<IconChar>(newValue.Substring(3));
			}
			catch (Exception)
			{
				Logging.Shared.Warning($"Font Awesome icon {newValue} not found");
				this.Content = IconChar.Question;
			}
		}
		else
		{
			this.Content = $"pack://application:,,,/StudioFourteen;component/Assets/Icons/{newValue}.svg";
		}
	}
}

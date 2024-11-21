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

namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils.Controls;

using IconBlock = WpfUtils.Controls.IconBlock;

[DependencyProperty<Key>("Key", DefaultValue = Key.LeftShift)]
public partial class KeyPrompt : UserControl
{
	public KeyPrompt()
	{
		this.InitializeComponent();
	}

	partial void OnKeyChanged(Key newValue)
	{
		this.Label.Text = null;
		this.IconDisplay.Icon = IconChar.Question;
		this.IconRotation.Angle = 0;

		if (newValue == Key.Return)
		{
			this.IconDisplay.Icon = IconChar.LevelDown;
			this.IconRotation.Angle = 90;
		}
		else if (newValue == Key.Tab)
		{
			this.IconDisplay.Icon = IconChar.Exchange;
		}
		else if (newValue == Key.LeftShift)
		{
			this.IconDisplay.Icon = IconChar.ArrowUp;
			this.Label.Text = "L";
		}
		else if (newValue == Key.RightShift)
		{
			this.IconDisplay.Icon = IconChar.ArrowUp;
			this.Label.Text = "R";
		}
		else
		{
			this.IconDisplay.Visibility = System.Windows.Visibility.Collapsed;
			this.Label.Text = newValue.ToString().ToUpper();
		}
	}
}
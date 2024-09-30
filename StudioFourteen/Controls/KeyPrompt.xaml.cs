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
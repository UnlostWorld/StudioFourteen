namespace ScreenshotStudio.Controls;

using DependencyPropertyGenerator;
using FontAwesome.Sharp.Pro;
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
		this.IconDisplay.Icon = ProIcons.Question;
		this.IconDisplay.IconStyle = IconBlock.IconStyles.Solid;
		this.IconRotation.Angle = 0;

		if (newValue == Key.Return)
		{
			this.IconDisplay.Icon = ProIcons.LevelDown;
			this.IconRotation.Angle = 90;
		}
		else if (newValue == Key.Tab)
		{
			this.IconDisplay.Icon = ProIcons.Exchange;
		}
		else if (newValue == Key.LeftShift)
		{
			this.IconDisplay.Icon = ProIcons.ArrowAltUp;
			this.IconDisplay.IconStyle = IconBlock.IconStyles.OutlineThin;
			this.Label.Text = "L";
		}
		else if (newValue == Key.RightShift)
		{
			this.IconDisplay.Icon = ProIcons.ArrowAltUp;
			this.IconDisplay.IconStyle = IconBlock.IconStyles.OutlineThin;
			this.Label.Text = "R";
		}
		else
		{
			this.IconDisplay.Visibility = System.Windows.Visibility.Collapsed;
			this.Label.Text = newValue.ToString().ToUpper();
		}
	}
}
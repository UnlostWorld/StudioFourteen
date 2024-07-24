namespace ScreenshotStudio.Controls;

using FontAwesome.Sharp.Pro;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils.Controls;
using WpfUtils.DependencyProperties;

using IconBlock = WpfUtils.Controls.IconBlock;

public partial class KeyPrompt : UserControl
{
	public static readonly IBind<Key> KeyDp = Binder.Register<Key, KeyPrompt>(nameof(Key), OnKeyChanged);

	public KeyPrompt()
	{
		this.InitializeComponent();
		this.Key = Key.LeftShift;
	}

	public Key Key
	{
		get => KeyDp.Get(this);
		set => KeyDp.Set(this, value);
	}

	private static void OnKeyChanged(KeyPrompt sender, Key key)
	{
		sender.Label.Text = null;
		sender.IconDisplay.Icon = ProIcons.Question;
		sender.IconDisplay.IconStyle = IconBlock.IconStyles.Solid;
		sender.IconRotation.Angle = 0;

		if (key == Key.Return)
		{
			sender.IconDisplay.Icon = ProIcons.LevelDown;
			sender.IconRotation.Angle = 90;
		}
		else if (key == Key.Tab)
		{
			sender.IconDisplay.Icon = ProIcons.Exchange;
		}
		else if (key == Key.LeftShift)
		{
			sender.IconDisplay.Icon = ProIcons.ArrowAltUp;
			sender.IconDisplay.IconStyle = IconBlock.IconStyles.OutlineThin;
			sender.Label.Text = "L";
		}
		else if (key == Key.RightShift)
		{
			sender.IconDisplay.Icon = ProIcons.ArrowAltUp;
			sender.IconDisplay.IconStyle = IconBlock.IconStyles.OutlineThin;
			sender.Label.Text = "R";
		}
		else
		{
			sender.IconDisplay.Visibility = System.Windows.Visibility.Collapsed;
			sender.Label.Text = key.ToString().ToUpper();
		}
	}
}
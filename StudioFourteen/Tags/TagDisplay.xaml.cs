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

namespace StudioFourteen.Tags;

using System.Windows;
using System.Windows.Controls;

public partial class TagDisplay : UserControl
{
	public static readonly DependencyProperty TagsProperty = DependencyProperty.Register(
		nameof(TagDisplay.Tags),
		typeof(TagCollection),
		typeof(TagDisplay),
		new(new TagCollection()));

	public TagDisplay()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public delegate void TagSelectedDelegate(Tag tag);

	public event TagSelectedDelegate? TagSelected;

	public TagCollection Tags
	{
		get => (TagCollection)this.GetValue(TagsProperty);
		set => this.SetValue(TagsProperty, value);
	}

	private void OnTagClicked(object sender, RoutedEventArgs e)
	{
		if (sender is Button btn && btn.DataContext is Tag tag)
		{
			this.TagSelected?.Invoke(tag);
		}
	}
}

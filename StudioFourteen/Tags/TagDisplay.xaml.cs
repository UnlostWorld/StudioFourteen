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

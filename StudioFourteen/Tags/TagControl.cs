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
using DependencyPropertyGenerator;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

[DependencyProperty<Tag>("Value")]
[DependencyProperty<Brush>("TagColor")]
[DependencyProperty<string>("TagName")]
[DependencyProperty<string>("TagToolTip")]
public partial class TagControl : Control
{
	private static readonly Dictionary<Dispatcher, ResourceDictionary> ColorDictionaries = new();

	private ResourceDictionary? colorDictionary = null;
	private Button? tagButton;
	private TagsControl? host;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		if (this.tagButton != null)
			this.tagButton.Click -= this.OnTagButtonClicked;

		this.tagButton = this.GetTemplateChild("PART_TagButton") as Button;

		if (this.tagButton != null)
			this.tagButton.Click += this.OnTagButtonClicked;

		this.host = this.FindParent<TagsControl>();

		if (this.colorDictionary == null)
		{
			lock (ColorDictionaries)
			{
				ResourceDictionary? colorDictionary;

				if (!ColorDictionaries.TryGetValue(this.Dispatcher, out colorDictionary))
				{
					colorDictionary = new();
					colorDictionary.Source = new("pack://application:,,,/StudioFourteen;component/Tags/TagColors.xaml");
					ColorDictionaries.Add(this.Dispatcher, colorDictionary);
				}

				this.colorDictionary = colorDictionary;
			}
		}

		this.UpdateColor();
	}

	partial void OnValueChanged(Tag? newValue)
	{
		if (newValue == null)
			return;

		this.TagName = newValue.DisplayName;
		this.TagToolTip = newValue.ToolTip;

		this.UpdateColor();
	}

	private void UpdateColor()
	{
		if (this.colorDictionary == null || this.Value == null)
			return;

		Brush? color = null;
		string colorName = $"TagColor_{this.Value.Name}";
		if (this.colorDictionary.Contains(colorName))
		{
			color = this.colorDictionary[colorName] as Brush;
		}

		this.TagColor = color;
	}

	private void OnTagButtonClicked(object sender, RoutedEventArgs e)
	{
		if (this.Value == null)
			return;

		this.host?.OnTagClicked(this.Value);
	}
}

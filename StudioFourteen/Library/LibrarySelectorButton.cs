namespace StudioFourteen.Library;

using DependencyPropertyGenerator;
using StudioFourteen.Tags;
using System;
using System.Windows.Controls;

[DependencyProperty<Type>("Type")]
[DependencyProperty<object>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<string>("Title")]
[DependencyProperty<TagCollection>("SearchTags")]
public partial class LibrarySelectorButton : Control
{
	private Button? button;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		if (this.button != null)
		{
			this.button.Click -= this.OnClicked;
		}

		this.button = this.GetTemplateChild("PART_Button") as Button;

		if (this.button != null)
		{
			this.button.Click += this.OnClicked;
		}
	}

	private void OnClicked(object sender, System.Windows.RoutedEventArgs e)
	{
		if (this.Type == null)
			return;

		TagCollection defaultTags = this.SearchTags ?? new();

		if (defaultTags.Count <= 0)
			defaultTags.Add("Named");

		MiniLibraryPopOut.Show(
			this,
			this.Title ?? string.Empty,
			defaultTags,
			this.Type,
			this.Value,
			(appearance, isFinal) =>
			{
				this.Dispatcher.Invoke(() =>
				{
					this.Value = appearance;
				});
			});
	}
}

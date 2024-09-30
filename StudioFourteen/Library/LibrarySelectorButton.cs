namespace StudioFourteen.Library;

using DependencyPropertyGenerator;
using StudioFourteen.Tags;
using System;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils.Commands;

[DependencyProperty<Type>("Type")]
[DependencyProperty<object>("Value")]
[DependencyProperty<ICommand>("Clicked")]
public partial class LibrarySelectorButton : Control
{
	public LibrarySelectorButton()
	{
		this.Clicked = new SimpleCommand(this.OnClicked);
	}

	private void OnClicked()
	{
		if (this.Type == null)
			return;

		TagCollection defaultTags = new();
		defaultTags.Add("Named");

		LibraryModal.Show(
			this,
			"Create Character",
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

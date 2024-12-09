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

namespace StudioFourteen.Library;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Tags;
using System;
using System.Windows.Controls;
using System.Windows.Input;

[DependencyProperty<Type>("Type")]
[DependencyProperty<object>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<string>("Title")]
[DependencyProperty<object>("IconBackground")]
[DependencyProperty<TagCollection>("SearchTags")]
public partial class LibrarySelectorButton : Control
{
	private Button? button;
	private LibraryContextMenu? menu;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		if (this.button != null)
		{
			this.button.Click -= this.OnClicked;
			this.button.MouseRightButtonUp -= this.OnMouseRightButtonUp;
			this.button.ToolTipOpening -= this.OnToolTipOpening;
		}

		this.button = this.GetTemplateChild("PART_Button") as Button;
		this.menu = this.GetTemplateChild("PART_Menu") as LibraryContextMenu;

		if (this.button != null)
		{
			this.button.Click += this.OnClicked;
			this.button.MouseRightButtonUp += this.OnMouseRightButtonUp;
			this.button.ToolTipOpening += this.OnToolTipOpening;
			this.button.MouseLeave += this.OnMouseLeave;
		}

		if (this.menu != null)
		{
			this.menu.OnCollectingMenus = this.CollectMenus;
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
			(newValue, isFinal) =>
			{
				this.Dispatcher.Invoke(() =>
				{
					this.Value = newValue;
				});
			});
	}

	// Hijack the tooltip logic.
	private void OnToolTipOpening(object sender, ToolTipEventArgs? e)
	{
		if (e != null)
			e.Handled = true;

		if (this.Value is LibraryEntryBase entry)
		{
			this.menu?.Enter(entry, this);
		}
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (this.Value is LibraryEntryBase entry)
		{
			this.menu?.Leave(entry);
		}
	}

	private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (this.Value is LibraryEntryBase entry)
		{
			this.menu?.Enter(entry, this);
			this.menu?.Expand();
		}
	}

	private void CollectMenus()
	{
		if (this.menu == null)
			return;

		// TODO: a provider for these icons, such as gear slots with "Equip racial"?
		////this.menu.AddMenu(IconChar.Eraser, "Clear", () => { this.Value = null; });
	}
}

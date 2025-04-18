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

namespace StudioFourteen.Library.Selector;

using DependencyPropertyGenerator;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Tags;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

[DependencyProperty<Type>("Type")]
[DependencyProperty<LibraryEntryBase>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<object>("Icon")]
[DependencyProperty<object>("IconBackground")]
[DependencyProperty<TagCollection>("SearchTags")]
[DependencyProperty<object>("PopOutHeader")]
[DependencyProperty<object>("PopOutHeaderTemplate")]
[DependencyProperty<object>("ToolTipHeader")]
[DependencyProperty<object>("ToolTipHeaderTemplate")]
[DependencyProperty<object>("PopOutBackgroundDetail")]
[DependencyProperty<object>("PopOutFooter")]
[DependencyProperty<object>("PopOutFooterTemplate")]
[DependencyProperty<bool>("IsClear", DefaultBindingMode = DefaultBindingMode.OneWayToSource)]
public partial class LibrarySelectorButton : Control
{
	private ButtonBase? button;
	private LibraryContextMenu? menu;
	private LibrarySelector? selector;

	protected ServiceManager Services => ServiceManager.Instance;

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();

		this.AllowDrop = true;

		if (this.button != null)
		{
			this.button.MouseUp -= this.OnMouseButtonUp;
			this.button.ToolTipOpening -= this.OnToolTipOpening;
			this.button.MouseLeave -= this.OnMouseLeave;
		}

		this.button = this.GetTemplateChild("PART_Button") as ButtonBase;
		this.menu = this.GetTemplateChild("PART_Menu") as LibraryContextMenu;
		this.selector = this.GetTemplateChild("PART_Selector") as LibrarySelector;

		if (this.button != null)
		{
			this.button.MouseUp += this.OnMouseButtonUp;
			this.button.ToolTipOpening += this.OnToolTipOpening;
			this.button.MouseLeave += this.OnMouseLeave;
		}
	}

	protected override void OnDragEnter(DragEventArgs e)
	{
		base.OnDragEnter(e);

		if (this.Services.DragAndDrop.CurrentDragObject is not LibraryEntryBase)
			return;

		Type type = this.Services.DragAndDrop.CurrentDragObject.GetType();
		if (!type.IsAssignableTo(this.Type))
			return;

		e.Effects = DragDropEffects.Move;
		e.Handled = true;
	}

	protected override void OnDrop(DragEventArgs e)
	{
		base.OnDrop(e);

		if (this.Services.DragAndDrop.CurrentDragObject is not LibraryEntryBase entry)
			return;

		Type type = this.Services.DragAndDrop.CurrentDragObject.GetType();
		if (!type.IsAssignableTo(this.Type))
			return;

		this.Value = entry;
		e.Handled = true;
	}

	partial void OnValueChanged()
	{
		this.Icon = this.Value?.Icon;
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

	private void OnMouseButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (e.ChangedButton == MouseButton.Middle)
		{
			this.IsClear = true;
			e.Handled = true;
		}
		else if (e.ChangedButton == MouseButton.Right)
		{
			if (this.Value is LibraryEntryBase entry)
			{
				this.menu?.Enter(entry, this);
				this.menu?.Expand();
				e.Handled = true;
			}
		}
	}
}

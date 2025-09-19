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

namespace StudioFourteen.Xaml;

using DependencyPropertyGenerator;
using System;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

[DependencyProperty<double>("MinimumX", DefaultValue = 0)]
[DependencyProperty<double>("MaximumX", DefaultValue = 1)]
[DependencyProperty<double>("MinimumY", DefaultValue = 0)]
[DependencyProperty<double>("MaximumY", DefaultValue = 1)]
[DependencyProperty<double>("ValueX", DefaultValue = 0.5)]
[DependencyProperty<double>("ValueY", DefaultValue = 0.5)]
[DependencyProperty<ControlTemplate>("SquareTemplate")]
[DependencyProperty<ControlTemplate>("CircularTemplate")]
[DependencyProperty<bool>("IsCircular")]
public partial class AreaSlider : Control
{
	private Thumb? thumb;
	private bool isClamping = false;

	public override void OnApplyTemplate()
	{
		if (this.thumb != null)
		{
			this.thumb.DragDelta -= this.OnThumbDragDelta;
		}

		base.OnApplyTemplate();
		this.thumb = this.GetTemplateChild("PART_Thumb") as Thumb;

		if (this.thumb != null)
		{
			this.thumb.DragDelta += this.OnThumbDragDelta;
		}
	}

	private void OnThumbDragDelta(object sender, DragDeltaEventArgs e)
	{
		if (this.thumb == null)
			return;

		double left = Canvas.GetLeft(this.thumb) + e.HorizontalChange;
		left = Math.Clamp(left, 0, this.ActualWidth - this.thumb.ActualWidth);

		double top = Canvas.GetTop(this.thumb) + e.VerticalChange;
		top = Math.Clamp(top, 0, this.ActualHeight - this.thumb.ActualHeight);

		this.ValueX = this.MinimumX + ((left / (this.ActualWidth - this.thumb.ActualWidth)) * (this.MaximumX - this.MinimumX));
		this.ValueY = this.MinimumY + ((top / (this.ActualHeight - this.thumb.ActualHeight)) * (this.MaximumY - this.MinimumY));
	}

	partial void OnIsCircularChanged()
	{
		this.Template = this.IsCircular ? this.CircularTemplate : this.SquareTemplate;
	}

	partial void OnCircularTemplateChanged()
	{
		if (this.IsCircular)
		{
			this.Template = this.CircularTemplate;
		}
	}

	partial void OnSquareTemplateChanged()
	{
		if (!this.IsCircular)
		{
			this.Template = this.SquareTemplate;
		}
	}

	partial void OnValueXChanged() => this.OnValueChanged();
	partial void OnValueYChanged() => this.OnValueChanged();

	private void OnValueChanged()
	{
		if (this.thumb == null)
			return;

		double y = (this.ValueY - this.MinimumY) / (this.MaximumY - this.MinimumY);
		double x = (this.ValueX - this.MinimumX) / (this.MaximumX - this.MinimumX);

		if (this.IsCircular && !this.isClamping)
		{
			double x2 = x - 0.5;
			double y2 = y - 0.5;
			double len = Math.Sqrt((x2 * x2) + (y2 * y2));
			if (len > 0.5)
			{
				x = 0.5 + ((x2 * 0.5) / len);
				y = 0.5 + ((y2 * 0.5) / len);

				this.isClamping = true;
				this.ValueX = this.MinimumX + (x * (this.MaximumX - this.MinimumX));
				this.ValueY = this.MinimumY + (y * (this.MaximumY - this.MinimumY));
				this.isClamping = false;
			}
		}

		Canvas.SetLeft(this.thumb, x * (this.ActualWidth - this.thumb.ActualWidth));
		Canvas.SetTop(this.thumb, y * (this.ActualHeight - this.thumb.ActualHeight));
	}
}

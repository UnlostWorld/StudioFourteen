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

using System.Windows;
using System.Windows.Input;
using StudioFourteen.Utilities;
using DependencyPropertyGenerator;
using System;

[DependencyProperty<bool>("IsMouseDown")]
[DependencyProperty<bool>("WrapMouseDrag", DefaultValue = true)]
[DependencyProperty<double>("Change", DefaultValue = 1)]
[DependencyProperty<double>("ChangeProgress", DefaultValue = 0)]
public partial class Slider : System.Windows.Controls.Slider
{
	private Point startPosition;
	private double trackingValue;

	protected ServiceManager Services => ServiceManager.Instance;

	protected double GetChangeMultiplier()
	{
		if (Keyboard.IsKeyDown(Key.LeftShift))
			return 10;

		if (Keyboard.IsKeyDown(Key.RightShift))
			return 10;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			return 0.1f;

		if (Keyboard.IsKeyDown(Key.RightCtrl))
			return 0.1f;

		return 1.0;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.IsMouseDown)
		{
			var newPos = CursorUtility.GetPosition();
			Vector delta = newPos - this.startPosition;

			double change = this.Change;
			if (change == 0)
				change = 1;

			double rate = change / 10;
			rate = (rate * delta.X) * this.GetChangeMultiplier();

			if (this.Services.Tablet.PenPressure > 0)
				rate *= this.Services.Tablet.PenPressure;

			this.trackingValue += rate;

			if (this.WrapMouseDrag)
			{
				double range = this.Maximum - this.Minimum;
				while (this.trackingValue > this.Maximum)
					this.trackingValue -= range;

				while (this.trackingValue < this.Minimum)
					this.trackingValue += range;
			}
			else
			{
				this.trackingValue = Math.Clamp(this.trackingValue, this.Minimum, this.Maximum);
			}

			this.Value = this.trackingValue;
			CursorUtility.SetPosition(this.startPosition);
			this.startPosition = CursorUtility.GetPosition();
		}
	}

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		this.startPosition = CursorUtility.GetPosition();
		this.IsMouseDown = true;
		this.trackingValue = this.Value;
		this.CaptureMouse();
		CursorUtility.SetCursorVisible(false);
		e.Handled = true;

		base.OnPreviewMouseDown(e);
	}

	protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
	{
		this.IsMouseDown = false;
		this.ReleaseMouseCapture();
		CursorUtility.SetPosition(this.startPosition);
		CursorUtility.SetCursorVisible(true);
		e.Handled = true;

		base.OnPreviewMouseUp(e);
	}

	protected override void OnValueChanged(double oldValue, double newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		double change = this.Change;
		if (change == 0)
			change = 1;

		double p = this.Value;
		p /= 10;
		p = p / change;

		if (p < 0)
		{
			p = 1 - (-p % 1);
		}
		else
		{
			p = p % 1;
		}

		this.ChangeProgress = p;
	}
}

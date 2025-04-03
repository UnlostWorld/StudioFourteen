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

namespace StudioFourteen.Controls;

using System.Windows;
using System.Windows.Input;
using StudioFourteen.Utilities;
using DependencyPropertyGenerator;

[DependencyProperty<bool>("IsMouseDown")]
[DependencyProperty<double>("Change", DefaultValue = 1)]
[DependencyProperty<double>("ChangeProgress", DefaultValue = 0)]
public partial class Slider : System.Windows.Controls.Slider
{
	private Point startPosition;

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
			this.Value += (rate * delta.X) * this.GetChangeMultiplier();
			CursorUtility.SetPosition(this.startPosition);
		}
	}

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		this.startPosition = CursorUtility.GetPosition();
		this.IsMouseDown = true;
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

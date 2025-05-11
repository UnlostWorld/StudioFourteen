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

using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using DependencyPropertyGenerator;
using StudioFourteen.Utilities;

[DependencyProperty<bool>("IsMouseDown")]
[DependencyProperty<double>("Minimum")]
[DependencyProperty<double>("Maximum")]
[DependencyProperty<double>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<double>("Change")]
[DependencyProperty<double>("ChangeProgress", DefaultValue = 0)]
[DependencyProperty<double>("ChangePosIntensity", DefaultValue = 0)]
[DependencyProperty<double>("ChangeNegIntensity", DefaultValue = 0)]
[DependencyProperty<bool>("Wrap", DefaultValue = true)]
[DependencyProperty<bool>("IsTextFocused")]
public partial class NumberBox : Control
{
	private readonly Storyboard intensityNegStoryboard;
	private readonly Storyboard intensityPosStoryboard;

	private Point startPosition;
	private double trackingValue;
	private FrameworkElement? clicker;
	private TextBox? textBox;
	private Button? downButton;
	private Button? upButton;
	private bool isDoubleClick = false;

	public NumberBox()
	{
		DoubleAnimation intensityNegAnimation = new();
		intensityNegAnimation.To = 0;
		intensityNegAnimation.From = 1;
		intensityNegAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
		intensityNegAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(intensityNegAnimation, this);
		Storyboard.SetTargetProperty(intensityNegAnimation, new PropertyPath(ChangeNegIntensityProperty));

		this.intensityNegStoryboard = new();
		this.intensityNegStoryboard.Children.Add(intensityNegAnimation);

		DoubleAnimation intensityPosAnimation = new();
		intensityPosAnimation.To = 0;
		intensityPosAnimation.From = 1;
		intensityPosAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(500));
		intensityPosAnimation.EasingFunction = new PowerEase()
		{
			EasingMode = EasingMode.EaseOut,
		};

		Storyboard.SetTarget(intensityPosAnimation, this);
		Storyboard.SetTargetProperty(intensityPosAnimation, new PropertyPath(ChangePosIntensityProperty));

		this.intensityPosStoryboard = new();
		this.intensityPosStoryboard.Children.Add(intensityPosAnimation);
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public override void OnApplyTemplate()
	{
		if (this.downButton != null)
			this.downButton.Click -= this.OnDownClicked;

		if (this.upButton != null)
			this.upButton.Click -= this.OnUpClicked;

		if (this.clicker != null)
		{
			this.clicker.MouseMove -= this.OnClickerMouseMove;
			this.clicker.MouseDown -= this.OnClickerMouseDown;
			this.clicker.MouseUp -= this.OnClickerMouseUp;
		}

		if (this.textBox != null)
		{
			this.textBox.GotFocus -= this.OnTextGotFocus;
			this.textBox.LostFocus -= this.OnTextLostFocus;
			this.textBox.PreviewKeyDown -= this.OnTextKey;
		}

		this.clicker = this.GetTemplateChild("PART_Clicker") as FrameworkElement;
		this.textBox = this.GetTemplateChild("PART_TextBox") as TextBox;
		this.downButton = this.GetTemplateChild("PART_DownButton") as Button;
		this.upButton = this.GetTemplateChild("PART_UpButton") as Button;

		if (this.downButton != null)
			this.downButton.Click += this.OnDownClicked;

		if (this.upButton != null)
			this.upButton.Click += this.OnUpClicked;

		if (this.clicker != null)
		{
			this.clicker.MouseMove += this.OnClickerMouseMove;
			this.clicker.MouseDown += this.OnClickerMouseDown;
			this.clicker.MouseUp += this.OnClickerMouseUp;
		}

		if (this.textBox != null)
		{
			this.textBox.GotFocus += this.OnTextGotFocus;
			this.textBox.LostFocus += this.OnTextLostFocus;
			this.textBox.PreviewKeyDown += this.OnTextKey;
		}

		base.OnApplyTemplate();
	}

	private void OnDoubleClick()
	{
		if (this.textBox == null)
			return;

		this.textBox.Focusable = true;
		this.textBox.Focus();
		Keyboard.Focus(this.textBox);

		this.textBox.SelectAll();
	}

	private void OnClickerMouseMove(object sender, MouseEventArgs e)
	{
		if (this.IsMouseDown)
		{
			var newPos = CursorUtility.GetPosition();
			Vector delta = newPos - this.startPosition;

			double rate = this.CalculateChange(delta.X);

			if (this.Services.Tablet.PenPressure > 0)
				rate *= this.Services.Tablet.PenPressure;

			this.trackingValue += rate;

			if (rate < 0)
			{
				this.intensityNegStoryboard.Begin();
			}
			else if (rate > 0)
			{
				this.intensityPosStoryboard.Begin();
			}

			if (this.Wrap)
			{
				double range = this.Maximum - this.Minimum;
				while(this.trackingValue > this.Maximum)
					this.trackingValue -= range;

				while(this.trackingValue < this.Minimum)
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

	private void OnClickerMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (this.clicker == null)
			return;

		if (e.ClickCount >= 2)
		{
			this.isDoubleClick = true;
			return;
		}

		this.Focus();
		Keyboard.Focus(this);

		this.startPosition = CursorUtility.GetPosition();
		this.IsMouseDown = true;
		this.trackingValue = this.Value;
		this.clicker.CaptureMouse();
		CursorUtility.SetCursorVisible(false);
		e.Handled = true;
	}

	private void OnClickerMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.clicker == null)
			return;

		if (this.isDoubleClick)
		{
			this.isDoubleClick = false;
			this.OnDoubleClick();
			return;
		}

		this.IsMouseDown = false;
		this.clicker.ReleaseMouseCapture();
		CursorUtility.SetPosition(this.startPosition);
		CursorUtility.SetCursorVisible(true);
		e.Handled = true;
	}

	partial void OnValueChanged(double oldValue, double newValue)
	{
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

		if (this.textBox == null)
			return;

		if (!this.textBox.IsFocused)
		{
			this.textBox.Text = newValue.ToString("0.###");
		}
	}

	private double CalculateChange(double delta)
	{
		double modifier = 1.0;
		if (Keyboard.IsKeyDown(Key.LeftShift))
			modifier = 10;

		if (Keyboard.IsKeyDown(Key.RightShift))
			modifier = 10;

		if (Keyboard.IsKeyDown(Key.LeftCtrl))
			modifier = 0.1;

		if (Keyboard.IsKeyDown(Key.RightCtrl))
			modifier = 0.1;

		double change = this.Change;
		if (change == 0)
			change = 1;

		double rate = change / 10;
		rate = (rate * delta) * modifier;

		return rate;
	}

	private void OnDownClicked(object sender, RoutedEventArgs e)
	{
		this.Value -= this.CalculateChange(1);
	}

	private void OnUpClicked(object sender, RoutedEventArgs e)
	{
		this.Value += this.CalculateChange(1);
	}

	private void OnTextGotFocus(object sender, RoutedEventArgs e)
	{
		this.IsTextFocused = true;
	}

	private void OnTextLostFocus(object sender, RoutedEventArgs e)
	{
		if (this.textBox == null)
			return;

		this.IsTextFocused = false;
		this.textBox.Focusable = false;

		this.ApplyInputText();
	}

	private void OnTextKey(object sender, KeyEventArgs e)
	{
		if (this.textBox == null)
			return;

		if (e.Key == Key.Enter)
		{
			this.ApplyInputText();
		}

		// TODO: Integrate with studio navigation system.
		else if (e.Key == Key.Up)
		{
			this.Value += this.CalculateChange(1);
			this.textBox.Text = this.Value.ToString("0.###");
			this.textBox.CaretIndex = int.MaxValue;
		}
		else if (e.Key == Key.Down)
		{
			this.Value -= this.CalculateChange(1);
			this.textBox.Text = this.Value.ToString("0.###");
			this.textBox.CaretIndex = int.MaxValue;
		}
	}

	private void ApplyInputText()
	{
		if (this.textBox == null)
			return;

		this.Value = Convert.ToDouble(new DataTable().Compute(this.textBox.Text, null));
		this.textBox.Text = this.Value.ToString("0.###");
		this.textBox.CaretIndex = int.MaxValue;
	}
}
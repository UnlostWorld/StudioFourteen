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

namespace StudioFourteen.Interface.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using StudioFourteen.Services.Input;
using StudioFourteen.Services.Input.Devices;
using StudioFourteen.Services.Tick;

public class NumberBox : TemplatedControl
{
	public static readonly StyledProperty<float> ValueProperty;
	public static readonly StyledProperty<string> TextProperty;

	private readonly Input2DListener dragListener;
	private bool supressChanges = false;
	private bool isDragging = false;

	static NumberBox()
	{
		ValueProperty = AvaloniaProperty.Register<NumberBox, float>(nameof(NumberBox.Value), default, false, BindingMode.TwoWay);
		TextProperty = AvaloniaProperty.Register<NumberBox, string>(nameof(NumberBox.Text), "0", false, BindingMode.TwoWay);
	}

	public NumberBox()
	{
		this.dragListener = new(
			InputAction.Handle_Right,
			InputAction.Handle_Left,
			InputAction.Handle_Down,
			InputAction.Handle_Up);
	}

	public float Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public string Text
	{
		get => this.GetValue(TextProperty);
		set => this.SetValue(TextProperty, value);
	}

	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		e.Handled = true;

		this.isDragging = true;
		this.dragListener.Enable();
		Studio.Input.Mouse?.LockCursor(true);

		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		e.Handled = true;

		this.isDragging = false;
		Studio.Input.Mouse?.LockCursor(false);

		this.dragListener.Disable();
		base.OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		if (this.isDragging)
		{
			float delta = this.dragListener.Value.X;

			float newValue = this.Value - delta;
			this.SetCurrentValue(ValueProperty, newValue);
			e.Handled = true;
		}

		base.OnPointerMoved(e);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (!this.supressChanges)
		{
			this.supressChanges = true;
			if (change.Property == ValueProperty)
			{
				this.Text = this.Value.ToString("F3");
			}

			this.supressChanges = false;
		}

		base.OnPropertyChanged(change);
	}
}
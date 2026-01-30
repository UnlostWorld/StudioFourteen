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
using StudioFourteen.Services.Tick;

public class NumberBox : TemplatedControl
{
	public static readonly StyledProperty<float> ValueProperty;
	public static readonly StyledProperty<string> TextProperty;

	private PointerPoint? lastDragPosition;

	static NumberBox()
	{
		ValueProperty = AvaloniaProperty.Register<NumberBox, float>(nameof(NumberBox.Value), default, false, BindingMode.TwoWay);
		TextProperty = AvaloniaProperty.Register<NumberBox, string>(nameof(NumberBox.Text), "0", false, BindingMode.TwoWay);
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

		Studio.Input.Mouse?.LockCursor(true);

		this.lastDragPosition = e.GetCurrentPoint(this);
		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		e.Handled = true;

		Studio.Input.Mouse?.LockCursor(false);

		this.lastDragPosition = null;
		base.OnPointerReleased(e);
	}

	protected override void OnPointerMoved(PointerEventArgs e)
	{
		if (this.lastDragPosition != null)
		{
			PointerPoint p = e.GetCurrentPoint(this);
			Point delta = p.Position - this.lastDragPosition.Value.Position;
			this.lastDragPosition = p;
			////Studio.Log.Information($">> {delta}");
			e.Handled = true;
		}

		base.OnPointerMoved(e);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		base.OnPropertyChanged(change);

		if (change.Property == ValueProperty)
		{
			this.Text = this.Value.ToString("F3");
		}
	}
}
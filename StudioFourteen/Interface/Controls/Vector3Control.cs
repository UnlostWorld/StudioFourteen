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

using System.Numerics;
using Avalonia;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Media;
using StudioFourteen.Services.Rendering;
using Color = Avalonia.Media.Color;

public class Vector3Control : TemplatedControl
{
	public static readonly StyledProperty<Vector3> ValueProperty;
	public static readonly StyledProperty<float> XProperty;
	public static readonly StyledProperty<float> YProperty;
	public static readonly StyledProperty<float> ZProperty;

	public static readonly StyledProperty<IBrush> XForegroundProperty;
	public static readonly StyledProperty<IBrush> YForegroundProperty;
	public static readonly StyledProperty<IBrush> ZForegroundProperty;

	private bool supressChanges = false;

	static Vector3Control()
	{
		ValueProperty = AvaloniaProperty.Register<Vector3Control, Vector3>(nameof(Vector3Control.Value), default, false, BindingMode.TwoWay);
		XProperty = AvaloniaProperty.Register<Vector3Control, float>(nameof(Vector3Control.X), default, false, BindingMode.TwoWay);
		YProperty = AvaloniaProperty.Register<Vector3Control, float>(nameof(Vector3Control.Y), default, false, BindingMode.TwoWay);
		ZProperty = AvaloniaProperty.Register<Vector3Control, float>(nameof(Vector3Control.Z), default, false, BindingMode.TwoWay);

		XForegroundProperty = AvaloniaProperty.Register<Vector3Control, IBrush>(nameof(Vector3Control.XForeground));
		YForegroundProperty = AvaloniaProperty.Register<Vector3Control, IBrush>(nameof(Vector3Control.YForeground));
		ZForegroundProperty = AvaloniaProperty.Register<Vector3Control, IBrush>(nameof(Vector3Control.ZForeground));
	}

	public Vector3 Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public float X
	{
		get => this.GetValue(XProperty);
		set => this.SetValue(XProperty, value);
	}

	public float Y
	{
		get => this.GetValue(YProperty);
		set => this.SetValue(YProperty, value);
	}

	public float Z
	{
		get => this.GetValue(ZProperty);
		set => this.SetValue(ZProperty, value);
	}

	public IBrush XForeground
	{
		get => this.GetValue(XForegroundProperty);
		set => this.SetValue(XForegroundProperty, value);
	}

	public IBrush YForeground
	{
		get => this.GetValue(YForegroundProperty);
		set => this.SetValue(YForegroundProperty, value);
	}

	public IBrush ZForeground
	{
		get => this.GetValue(ZForegroundProperty);
		set => this.SetValue(ZForegroundProperty, value);
	}

	protected override void OnLoaded(RoutedEventArgs e)
	{
		base.OnLoaded(e);

		this.XForeground = new SolidColorBrush(Axes.XColor.ToAvalonia());
		this.YForeground = new SolidColorBrush(Axes.YColor.ToAvalonia());
		this.ZForeground = new SolidColorBrush(Axes.ZColor.ToAvalonia());
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (!this.supressChanges)
		{
			this.supressChanges = true;
			if (change.Property == ValueProperty)
			{
				this.SetCurrentValue(XProperty, this.Value.X);
				this.SetCurrentValue(YProperty, this.Value.Y);
				this.SetCurrentValue(ZProperty, this.Value.Z);
			}
			else if (change.Property == XProperty)
			{
				Vector3 v = this.Value;
				v.X = this.X;
				this.SetCurrentValue(ValueProperty, v);
			}
			else if (change.Property == YProperty)
			{
				Vector3 v = this.Value;
				v.Y = this.Y;
				this.SetCurrentValue(ValueProperty, v);
			}
			else if (change.Property == ZProperty)
			{
				Vector3 v = this.Value;
				v.Z = this.Z;
				this.SetCurrentValue(ValueProperty, v);
			}

			this.supressChanges = false;
		}

		base.OnPropertyChanged(change);
	}
}
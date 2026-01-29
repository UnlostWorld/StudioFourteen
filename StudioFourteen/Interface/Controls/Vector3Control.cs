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

public class Vector3Control : TemplatedControl
{
	public static readonly StyledProperty<Vector3?> ValueProperty;
	public static readonly StyledProperty<float?> XProperty;
	public static readonly StyledProperty<float?> YProperty;
	public static readonly StyledProperty<float?> ZProperty;

	static Vector3Control()
	{
		ValueProperty = AvaloniaProperty.Register<Vector3Control, Vector3?>(nameof(Vector3Control.Value));
		XProperty = AvaloniaProperty.Register<Vector3Control, float?>(nameof(Vector3Control.X));
		YProperty = AvaloniaProperty.Register<Vector3Control, float?>(nameof(Vector3Control.Y));
		ZProperty = AvaloniaProperty.Register<Vector3Control, float?>(nameof(Vector3Control.Z));
	}

	public Vector3? Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public float? X
	{
		get => this.GetValue(XProperty);
		set => this.SetValue(XProperty, value);
	}

	public float? Y
	{
		get => this.GetValue(YProperty);
		set => this.SetValue(YProperty, value);
	}

	public float? Z
	{
		get => this.GetValue(ZProperty);
		set => this.SetValue(ZProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		if (change.Property == ValueProperty)
		{
			if (this.Value == null)
			{
				this.X = null;
				this.Y = null;
				this.Z = null;
			}
			else
			{
				this.X = this.Value.Value.X;
				this.Y = this.Value.Value.Y;
				this.Z = this.Value.Value.Z;
			}
		}

		base.OnPropertyChanged(change);
	}
}
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

using System;
using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using StudioFourteen.Services.Numerics;

public class TransformControl : TemplatedControl
{
	public static readonly StyledProperty<Transform> ValueProperty;

	public static readonly StyledProperty<Vector3> TranslationProperty;
	public static readonly StyledProperty<Quaternion> RotationProperty;
	public static readonly StyledProperty<Vector3> EulerProperty;
	public static readonly StyledProperty<Vector3> ScaleProperty;

	private bool supressChanges = false;

	static TransformControl()
	{
		ValueProperty = AvaloniaProperty.Register<TransformControl, Transform>(nameof(TransformControl.Value), default, false, BindingMode.TwoWay);
		TranslationProperty = AvaloniaProperty.Register<TransformControl, Vector3>(nameof(TransformControl.Translation), default, false, BindingMode.TwoWay);
		RotationProperty = AvaloniaProperty.Register<TransformControl, Quaternion>(nameof(TransformControl.Rotation), default, false, BindingMode.TwoWay);
		EulerProperty = AvaloniaProperty.Register<TransformControl, Vector3>(nameof(TransformControl.Euler), default, false, BindingMode.TwoWay);
		ScaleProperty = AvaloniaProperty.Register<TransformControl, Vector3>(nameof(TransformControl.Scale), default, false, BindingMode.TwoWay);
	}

	public Transform Value
	{
		get => this.GetValue(ValueProperty);
		set => this.SetValue(ValueProperty, value);
	}

	public Vector3 Translation
	{
		get => this.GetValue(TranslationProperty);
		set => this.SetValue(TranslationProperty, value);
	}

	public Quaternion Rotation
	{
		get => this.GetValue(RotationProperty);
		set => this.SetValue(RotationProperty, value);
	}

	public Vector3 Euler
	{
		get => this.GetValue(EulerProperty);
		set => this.SetValue(EulerProperty, value);
	}

	public Vector3 Scale
	{
		get => this.GetValue(ScaleProperty);
		set => this.SetValue(ScaleProperty, value);
	}

	protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
	{
		try
		{
			if (!this.supressChanges)
			{
				this.supressChanges = true;
				if (change.Property == ValueProperty)
				{
					Vector3 translation;
					Quaternion rotation;
					Vector3 scale;
					if (this.Value.ToTRS(out translation, out rotation, out scale))
					{
						this.SetCurrentValue(TranslationProperty, translation);
						this.SetCurrentValue(RotationProperty, rotation);
						this.SetCurrentValue(ScaleProperty, scale);
						this.SetCurrentValue(EulerProperty, rotation.ToEuler());
					}
				}
				else if (change.Property == TranslationProperty
					|| change.Property == RotationProperty
					|| change.Property == ScaleProperty)
				{
					this.SetCurrentValue(
						ValueProperty,
						Transform.FromTRS(this.Translation, this.Rotation, this.Scale));
				}

				this.supressChanges = false;
			}

			base.OnPropertyChanged(change);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error in property changed");
		}
	}
}
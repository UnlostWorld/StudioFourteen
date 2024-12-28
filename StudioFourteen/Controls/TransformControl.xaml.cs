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

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Posing;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;

[DependencyProperty<Transform>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]

[DependencyProperty<bool>("TranslationExpand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<Vector3>("Translation")]
[DependencyProperty<double>("TranslationMaximum")]
[DependencyProperty<double>("TranslationMinimum")]
[DependencyProperty<float>("TranslationRange", DefaultValue = 100)]
[DependencyProperty<float>("TranslationSmallChange", DefaultValue = 1)]
[DependencyProperty<float>("TranslationLargeChange", DefaultValue = 10)]
[DependencyProperty<float>("TranslationTickFrequency", DefaultValue = 5)]
[DependencyProperty<int>("TranslationDecimalPlaces", DefaultValue = 3)]

[DependencyProperty<bool>("RotationExpand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<Vector3>("RotationEuler")]
[DependencyProperty<double>("RotationMaximum", DefaultValue = 179.9f)]
[DependencyProperty<double>("RotationMinimum", DefaultValue = -179.9f)]
[DependencyProperty<float>("RotationRange", DefaultValue = 360)]
[DependencyProperty<float>("RotationSmallChange", DefaultValue = 1)]
[DependencyProperty<float>("RotationLargeChange", DefaultValue = 10)]
[DependencyProperty<float>("RotationTickFrequency", DefaultValue = 5)]
[DependencyProperty<int>("RotationDecimalPlaces", DefaultValue = 1)]

[DependencyProperty<bool>("ScaleExpand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<Vector3>("Scale")]
[DependencyProperty<double>("ScaleMaximum")]
[DependencyProperty<double>("ScaleMinimum", DefaultValue = 0.1f)]
[DependencyProperty<float>("ScaleRange", DefaultValue = 100)]
[DependencyProperty<float>("ScaleSmallChange", DefaultValue = 1)]
[DependencyProperty<float>("ScaleLargeChange", DefaultValue = 10)]
[DependencyProperty<float>("ScaleTickFrequency", DefaultValue = 5)]
[DependencyProperty<int>("ScaleDecimalPlaces", DefaultValue = 3)]
public partial class TransformControl : View
{
	private bool isUpdatingValue = false;
	private bool isUpdatingComponent = false;

	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.OnValueChanged();
	}

	partial void OnValueChanged()
	{
		if (this.isUpdatingComponent)
			return;

		this.isUpdatingValue = true;

		this.Translation = this.Value.Translation;
		this.RotationEuler = this.Value.Rotation.ToEuler();
		this.Scale = this.Value.Scale;

		this.isUpdatingValue = false;
	}

	partial void OnTranslationChanged(Vector3 oldValue, Vector3 newValue)
	{
		if (this.isUpdatingValue)
			return;

		this.isUpdatingComponent = true;

		Transform t = this.Value;
		t.Translation = newValue;
		this.Value = t;

		this.isUpdatingComponent = false;
	}

	partial void OnRotationEulerChanged(Vector3 oldValue, Vector3 newValue)
	{
		if (this.isUpdatingValue || this.isUpdatingComponent)
			return;

		this.isUpdatingComponent = true;

		Quaternion from = this.Value.Rotation;
		Quaternion to = QuaternionExtensions.FromEuler(newValue);
		Quaternion delta = Quaternion.Normalize(Quaternion.Inverse(from) * to);

		this.Value = Transform.FromRotation(delta) * this.Value;

		this.RotationEuler = Vector3.Zero;
		this.RotationEuler = this.Value.Rotation.ToEuler();

		this.isUpdatingComponent = false;
	}

	partial void OnScaleChanged(Vector3 oldValue, Vector3 newValue)
	{
		if (this.isUpdatingValue)
			return;

		this.isUpdatingComponent = true;

		Vector3 from = this.Value.Scale;
		Vector3 to = newValue;
		Vector3 delta = to / from;

		this.Value = Transform.FromScale(delta) * this.Value;

		this.isUpdatingComponent = false;
	}
}

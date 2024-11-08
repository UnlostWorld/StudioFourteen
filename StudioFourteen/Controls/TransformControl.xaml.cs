namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using StudioFourteen.Mvm;
using StudioFourteen.Posing;
using StudioFourteen.Structs.Extensions;
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

		Transform t = this.Value;
		Quaternion q = this.Value.Rotation;
		q.FromEuler(newValue);
		t.Rotation = q;
		this.Value = t;

		this.RotationEuler = Vector3.Zero;
		this.RotationEuler = this.Value.Rotation.ToEuler();

		this.isUpdatingComponent = false;
	}

	partial void OnScaleChanged(Vector3 oldValue, Vector3 newValue)
	{
		if (this.isUpdatingValue)
			return;

		this.isUpdatingComponent = true;

		Transform t = this.Value;
		t.Scale = newValue;
		this.Value = t;

		this.isUpdatingComponent = false;
	}
}

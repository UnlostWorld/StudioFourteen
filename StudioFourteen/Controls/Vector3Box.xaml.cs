namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using System.Numerics;
using System.Windows.Input;

[DependencyProperty<Vector3>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<bool>("Expand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<float>("ValueX")]
[DependencyProperty<float>("ValueY")]
[DependencyProperty<float>("ValueZ")]
[DependencyProperty<IconChar>("Icon")]
[DependencyProperty<double>("LargeChange")]
[DependencyProperty<double>("SmallChange")]
[DependencyProperty<double>("Range", DefaultValue = 100)]
[DependencyProperty<double>("TickFrequency", DefaultValue = 1)]
[DependencyProperty<bool>("Wrap")]
[DependencyProperty<double>("Minimum")]
[DependencyProperty<double>("Maximum")]
[DependencyProperty<int>("DecimalPlaces")]
[DependencyProperty<bool>("IsRelative")]
public partial class Vector3Box : View
{
	private Vector3? trackingValue;
	private bool isUpdatingValue = false;

	public Vector3 TrackingOrLiveValue
	{
		get
		{
			if (this.trackingValue != null)
				return (Vector3)this.trackingValue;

			return this.Value;
		}
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.OnValueChanged();
	}

	partial void OnValueChanged()
	{
		this.isUpdatingValue = true;

		this.ValueX = this.TrackingOrLiveValue.X;
		this.ValueY = this.TrackingOrLiveValue.Y;
		this.ValueZ = this.TrackingOrLiveValue.Z;

		this.isUpdatingValue = false;
	}

	partial void OnValueXChanged(float oldValue, float newValue)
	{
		if (this.isUpdatingValue)
			return;

		Vector3 t = this.TrackingOrLiveValue;
		t.X = newValue;
		this.Value = t;

		if (this.trackingValue != null)
		{
			this.trackingValue = t;
		}
	}

	partial void OnValueYChanged(float oldValue, float newValue)
	{
		if (this.isUpdatingValue)
			return;

		Vector3 t = this.TrackingOrLiveValue;
		t.Y = newValue;
		this.Value = t;

		if (this.trackingValue != null)
		{
			this.trackingValue = t;
		}
	}

	partial void OnValueZChanged(float oldValue, float newValue)
	{
		if (this.isUpdatingValue)
			return;

		Vector3 t = this.TrackingOrLiveValue;
		t.Z = newValue;
		this.Value = t;

		if (this.trackingValue != null)
		{
			this.trackingValue = t;
		}
	}

	private void OnSliderMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingValue = this.Value;
	}

	private void OnSliderMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingValue = null;
		this.OnValueChanged();
	}
}
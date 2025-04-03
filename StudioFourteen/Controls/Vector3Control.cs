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
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

[DependencyProperty<Vector3>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<bool>("Expand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<float>("ValueX", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<float>("ValueY", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<float>("ValueZ", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<double>("Change", DefaultValue = 1)]
[DependencyProperty<bool>("Wrap")]
[DependencyProperty<double>("Minimum")]
[DependencyProperty<double>("Maximum")]
[DependencyProperty<int>("DecimalPlaces")]
[DependencyProperty<bool>("IsRelative")]
[DependencyProperty<Style>("NumberSliderStyle")]
public partial class Vector3Control : Control
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

	protected virtual void OnInternalValueChanged(Vector3 newValue)
	{
	}

	partial void OnValueChanged()
	{
		this.isUpdatingValue = true;

		this.ValueX = this.TrackingOrLiveValue.X;
		this.ValueY = this.TrackingOrLiveValue.Y;
		this.ValueZ = this.TrackingOrLiveValue.Z;

		this.isUpdatingValue = false;

		this.OnInternalValueChanged(this.Value);
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

	/*private void OnSliderMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.trackingValue = this.Value;
	}

	private void OnSliderMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.trackingValue = null;
		this.OnValueChanged();
	}*/
}
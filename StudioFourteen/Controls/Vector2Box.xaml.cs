// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Controls;

using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Mvm;
using System.Numerics;
using System.Windows.Input;

[DependencyProperty<Vector2>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<bool>("Expand", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<float>("ValueX")]
[DependencyProperty<float>("ValueY")]
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
public partial class Vector2Box : View
{
	private Vector2? trackingValue;
	private bool isUpdatingValue = false;

	public Vector2 TrackingOrLiveValue
	{
		get
		{
			if (this.trackingValue != null)
				return (Vector2)this.trackingValue;

			return this.Value;
		}
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.OnValueChanged();
	}

	protected virtual void OnInternalValueChanged(Vector2 newValue)
	{
	}

	partial void OnValueChanged()
	{
		this.isUpdatingValue = true;

		this.ValueX = this.TrackingOrLiveValue.X;
		this.ValueY = this.TrackingOrLiveValue.Y;

		this.isUpdatingValue = false;

		this.OnInternalValueChanged(this.Value);
	}

	partial void OnValueXChanged(float oldValue, float newValue)
	{
		if (this.isUpdatingValue)
			return;

		Vector2 t = this.TrackingOrLiveValue;
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

		Vector2 t = this.TrackingOrLiveValue;
		t.Y = newValue;
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
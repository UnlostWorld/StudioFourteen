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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Structs.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

[DependencyProperty<Quaternion>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class QuaternionControl : MultiNumberSliderControl
{
	public QuaternionControl()
	{
		this.AddChannel(
			() => "X:",
			() => this.EulerValue.X,
			(v) =>
			{
				Vector3 val = this.EulerValue;
				val.X = (float)v;
				this.EulerValue = val;
			});

		this.AddChannel(
			() => "Y:",
			() => this.EulerValue.Y,
			(v) =>
			{
				Vector3 val = this.EulerValue;
				val.Y = (float)v;
				this.EulerValue = val;
			});

		this.AddChannel(
			() => "Z:",
			() => this.EulerValue.Z,
			(v) =>
			{
				Vector3 val = this.EulerValue;
				val.Z = (float)v;
				this.EulerValue = val;
			});
	}

	private Vector3 EulerValue
	{
		get
		{
			return this.Value.ToEuler();
		}

		set
		{
			Quaternion q = this.Value;
			q.FromEuler(value);
			this.Value = q;
		}
	}

	partial void OnValueChanged() => this.OnControlValueChanged();
}

[DependencyProperty<Vector2>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class Vector2Control : MultiNumberSliderControl
{
	public Vector2Control()
	{
		this.AddChannel(
			() => "X:",
			() => this.Value.X,
			(v) =>
			{
				Vector2 val = this.Value;
				val.X = (float)v;
				this.Value = val;
			});

		this.AddChannel(
			() => "Y:",
			() => this.Value.Y,
			(v) =>
			{
				Vector2 val = this.Value;
				val.Y = (float)v;
				this.Value = val;
			});
	}

	partial void OnValueChanged() => this.OnControlValueChanged();
}

[DependencyProperty<Vector3>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
public partial class Vector3Control : MultiNumberSliderControl
{
	public Vector3Control()
	{
		this.AddChannel(
			() => "X:",
			() => this.Value.X,
			(v) =>
			{
				Vector3 val = this.Value;
				val.X = (float)v;
				this.Value = val;
			});

		this.AddChannel(
			() => "Y:",
			() => this.Value.Y,
			(v) =>
			{
				Vector3 val = this.Value;
				val.Y = (float)v;
				this.Value = val;
			});

		this.AddChannel(
			() => "Z:",
			() => this.Value.Z,
			(v) =>
			{
				Vector3 val = this.Value;
				val.Z = (float)v;
				this.Value = val;
			});
	}

	partial void OnValueChanged() => this.OnControlValueChanged();
}

[DependencyProperty<double>("Change", DefaultValue = 1)]
[DependencyProperty<bool>("Wrap", DefaultValue = true)]
[DependencyProperty<double>("Minimum", DefaultValue = double.MinValue)]
[DependencyProperty<double>("Maximum", DefaultValue = double.MaxValue)]
[DependencyProperty<int>("DecimalPlaces", DefaultValue = 3)]
[DependencyProperty<Style>("NumberSliderStyle")]
[DependencyProperty<ObservableCollection<MultiNumberSliderControlChannel>>("Channels")]
public abstract partial class MultiNumberSliderControl
	: Control
{
	public void AddChannel(Func<string> getLabel, Func<double> getValue, Action<double> setValue)
	{
		if (this.Channels == null)
			this.Channels = new();

		this.Channels.Add(new MultiNumberSliderControlChannel(this, getLabel, getValue, setValue));
	}

	public void OnControlValueChanged()
	{
		if (this.Channels == null)
			return;

		foreach (MultiNumberSliderControlChannel channel in this.Channels)
		{
			channel.OnControlValueChanged();
		}
	}

	partial void OnMinimumChanged() => this.OnPropertiesChanged(nameof(this.Minimum));
	partial void OnMaximumChanged() => this.OnPropertiesChanged(nameof(this.Maximum));
	partial void OnDecimalPlacesChanged() => this.OnPropertiesChanged(nameof(this.DecimalPlaces));
	partial void OnNumberSliderStyleChanged() => this.OnPropertiesChanged(nameof(this.NumberSliderStyle));

	private void OnPropertiesChanged(string name)
	{
		if (this.Channels == null)
			return;

		foreach (MultiNumberSliderControlChannel channel in this.Channels)
		{
			channel.OnControlPropertyChanged(name);
		}
	}
}

public partial class MultiNumberSliderControlChannel(MultiNumberSliderControl control, Func<string> getLabel, Func<double> getValue, Action<double> setValue)
	: INotifyPropertyChanged
{
	public event PropertyChangedEventHandler? PropertyChanged;

	public string Label => getLabel.Invoke();

	public double Value
	{
		get => getValue.Invoke();
		set => setValue.Invoke(value);
	}

	public double Change => control.Change;
	public bool Wrap => control.Wrap;
	public double Minimum => control.Minimum;
	public double Maximum => control.Maximum;
	public int DecimalPlaces => control.DecimalPlaces;
	public Style? NumberSliderStyle => control.NumberSliderStyle;

	public void OnControlValueChanged()
	{
		this.PropertyChanged?.Invoke(this, new(nameof(this.Value)));
	}

	public void OnControlPropertyChanged(string name)
	{
		this.PropertyChanged?.Invoke(this, new(name));
	}
}
namespace ScreenshotStudio.Appearance.Customize;

using DependencyPropertyGenerator;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;

using CustomizeFacialFeatures = FFXIVClientStructs.FFXIV.Client.Game.Character.CustomizeDataExtensions.FacialFeatures;

[DependencyProperty<CustomizeFacialFeatures>("Value", DefaultValue = CustomizeFacialFeatures.None, DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<CharaMakeType.FacialFeatureOptions>("FacialFeatures")]
public partial class CustomizeFacialFeaturesOption : UserControl
{
	public CustomizeFacialFeaturesOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public ObservableCollection<Option> Options { get; init; } = new();

	partial void OnValueChanged(CustomizeFacialFeatures newValue)
	{
		foreach (Option op in this.Options)
		{
			op.Notify();
		}
	}

	partial void OnFacialFeaturesChanged(CharaMakeType.FacialFeatureOptions? newValue)
	{
		this.Options.Clear();

		if (newValue == null)
			return;

		foreach (CharaMakeType.FacialFeatureOptions.Option option in newValue.Options)
		{
			Option op = new Option(this);
			op.Value = option.Value;
			op.Icon = option.Icon;
			this.Options.Add(op);
		}
	}

	public class Option : INotifyPropertyChanged
	{
		public Option(CustomizeFacialFeaturesOption owner)
		{
			this.Owner = owner;
		}

		public event PropertyChangedEventHandler? PropertyChanged;

		public CustomizeFacialFeaturesOption Owner { get; init; }
		public CustomizeFacialFeatures Value { get; set; }
		public ImageReference? Icon { get; set; }

		public bool IsLegacy => this.Value == CustomizeFacialFeatures.LegacyTattoo;

		public bool IsEnabled
		{
			get => this.Owner.Value.HasFlag(this.Value);
			set
			{
				if (value)
				{
					this.Owner.Value |= this.Value;
				}
				else
				{
					this.Owner.Value &= ~this.Value;
				}
			}
		}

		public void Notify()
		{
			this.PropertyChanged?.Invoke(this, new(nameof(this.IsEnabled)));
			this.PropertyChanged?.Invoke(this, new(nameof(this.Icon)));
		}
	}
}

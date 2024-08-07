namespace ScreenshotStudio.Studio.Customize;

using DependencyPropertyGenerator;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

[DependencyProperty<byte>("Value", DefaultBindingMode = DefaultBindingMode.TwoWay)]
[DependencyProperty<CharaMakeType.Menu>("Menu")]
[DependencyProperty<CornerRadius>("CornerRadius")]
public partial class CustomizeColorOption : UserControl, INotifyPropertyChanged
{
	public CustomizeColorOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.CornerRadius = new CornerRadius(6);
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public List<ColorOption> Options { get; init; } = new();

	public ColorOption? SelectedOption
	{
		get
		{
			foreach (var option in this.Options)
			{
				if (option.Value == this.Value)
				{
					return option;
				}
			}

			return null;
		}

		set => this.Value = value?.Value ?? 0;
	}

	partial void OnValueChanged(byte newValue)
	{
		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.SelectedOption)));
	}

	partial void OnMenuChanged(CharaMakeType.Menu? newValue)
	{
		this.PopulateColors();
		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.SelectedOption)));
		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.Value)));
	}

	private void PopulateColors()
	{
		this.Options.Clear();

		if (this.Menu != null)
		{
			HumanCmp.Entry[]? entries = HumanCmp.Get(this.Menu);
			if (entries != null)
			{
				for (byte j = 0; j < this.Menu.NumOptions; ++j)
				{
					if (entries != null && j < entries.Length)
					{
						this.Options.Add(new((byte)(this.Menu.Min + j), entries[j]));
					}
				}
			}
		}

		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.Options)));
	}

	public class ColorOption(byte value, HumanCmp.Entry entry)
	{
		public byte Value { get; init; } = value;
		public HumanCmp.Entry Entry { get; init; } = entry;
	}
}

namespace ScreenshotStudio.Studio.Customize;

using Anamnesis.Actor.Utilities;
using ScreenshotStudio.GameData.Excel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using WpfUtils.DependencyProperties;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkComponentNumericInput.Delegates;

public partial class CustomizeColorOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeColorOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeColorOption>(nameof(Menu), OnMenuChanged);
	public static IBind<CornerRadius> CornerRadiusDp = Binder.Register<CornerRadius, CustomizeColorOption>(nameof(CornerRadius));

	public CustomizeColorOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
		this.CornerRadius = new(6, 6, 6, 6);
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

	public byte Value
	{
		get => ValueDp.Get(this);
		set => ValueDp.Set(this, value);
	}

	public CharaMakeType.Menu? Menu
	{
		get => MenuDp.Get(this);
		set => MenuDp.Set(this, value);
	}

	public CornerRadius CornerRadius
	{
		get => CornerRadiusDp.Get(this);
		set
		{
			CornerRadiusDp.Set(this, value);
			this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.LeftElementCornerRadius)));
			this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.RightElementCornerRadius)));
		}
	}

	public CornerRadius LeftElementCornerRadius => new(this.CornerRadius.TopLeft, 0, 0, this.CornerRadius.BottomLeft);
	public CornerRadius RightElementCornerRadius => new(0, this.CornerRadius.TopRight, this.CornerRadius.BottomRight, 0);

	public static void OnValueChanged(CustomizeColorOption sender, byte newValue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.SelectedOption)));
	}

	public static void OnMenuChanged(CustomizeColorOption sender, CharaMakeType.Menu? newValue)
	{
		sender.PopulateColors();
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.SelectedOption)));
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.Value)));
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

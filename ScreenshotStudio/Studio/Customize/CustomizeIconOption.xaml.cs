namespace ScreenshotStudio.Studio.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel;
using ScreenshotStudio.GameData;
using ScreenshotStudio.GameData.Excel;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Controls;
using WpfUtils.DependencyProperties;

public partial class CustomizeIconOption : UserControl, INotifyPropertyChanged
{
	public static IBind<byte> ValueDp = Binder.Register<byte, CustomizeIconOption>(nameof(Value), OnValueChanged);
	public static IBind<CharaMakeType.Menu?> MenuDp = Binder.Register<CharaMakeType.Menu?, CustomizeIconOption>(nameof(Menu), OnMenuChanged);
	public static IBind<bool> FlippedDp = Binder.Register<bool, CustomizeIconOption>(nameof(Flipped));

	public CustomizeIconOption()
	{
		this.InitializeComponent();
		this.ContentArea.DataContext = this;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

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

	public bool Flipped
	{
		get => FlippedDp.Get(this);
		set => FlippedDp.Set(this, value);
	}

	public List<IconOption> Options { get; init; } = new();

	public IconOption? SelectedOption
	{
		get
		{
			foreach (var option in this.Options)
			{
				if (option.FeatureId == this.Value)
				{
					return option;
				}
			}

			return null;
		}

		set => this.Value = value?.FeatureId ?? 0;
	}

	public static void OnValueChanged(CustomizeIconOption sender, byte newValue)
	{
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.SelectedOption)));
	}

	public static void OnMenuChanged(CustomizeIconOption sender, CharaMakeType.Menu? newValue)
	{
		sender.PopulateOptions();
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.SelectedOption)));
		sender.PropertyChanged?.Invoke(sender, new(nameof(CustomizeColorOption.Value)));
	}

	private void PopulateOptions()
	{
		this.Options.Clear();
		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.Options)));

		if (this.Menu?.CustomizationIndex == CustomizeIndex.HairStyle || this.Menu?.CustomizationIndex == CustomizeIndex.Facepaint)
		{
			DataSheet<HairMakeType>? hairMakeTypeSheet = GameDataService.Get<HairMakeType>();

			if (hairMakeTypeSheet == null)
				return;

			foreach (HairMakeType? hairMakeType in hairMakeTypeSheet)
			{
				if (hairMakeType == null)
					continue;

				if (hairMakeType.Race != this.Menu?.Race || hairMakeType.Tribe != this.Menu?.Tribe || hairMakeType.Gender != this.Menu?.Gender)
					continue;

				CharaMakeCustomize?[] makeCustomizeOptions = hairMakeType.HairStyles;
				if (this.Menu.CustomizationIndex == CustomizeIndex.Facepaint)
					makeCustomizeOptions = hairMakeType.FacePaints;

				int length = (byte)makeCustomizeOptions.Length;
				for (byte j = 0; j < length; ++j)
				{
					CharaMakeCustomize? makeCustomize = makeCustomizeOptions[j];
					if (makeCustomize == null || makeCustomize.Icon == null || makeCustomize.Icon.ImageId == 0)
						continue;

					this.Options.Add(new(makeCustomize));
				}

				break;
			}
		}
		else if (this.Menu?.Icons != null)
		{
			for(int i = 0; i < this.Menu.Icons.Length; i++)
			{
				this.Options.Add(new((byte)(this.Menu.Min + i), this.Menu.Icons[i]));
			}
		}

		this.PropertyChanged?.Invoke(this, new(nameof(CustomizeColorOption.Options)));
	}

	public class IconOption
	{
		public IconOption(byte featureId, ImageReference icon)
		{
			this.FeatureId = featureId;
			this.Icon = icon;
		}

		public IconOption(CharaMakeCustomize customize)
		{
			this.Icon = customize.Icon;
			this.Item = customize.Item;
			this.FeatureId = customize.FeatureId;
		}

		public ImageReference? Icon { get; private set; }
		public Item? Item { get; private set; }
		public byte FeatureId { get; private set; }
	}
}

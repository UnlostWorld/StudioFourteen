// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.GameData.Sheets;
using ScreenshotStudio.Structs;
using System.Collections.Generic;
using LuminaData = Lumina.GameData;

// Much of this has been taken from  Ottermandias / Glamourer.
// https://github.com/Ottermandias/Glamourer/blob/main/Glamourer.GameData/Customization/CharaMakeParams.cs
[Sheet("CharaMakeType", columnHash: 0x80d7db6d)]
public class CharaMakeType : ExcelRow
{
	public const int NumMenus = 28;
	public const int NumVoices = 12;
	public const int NumGraphics = 10;
	public const int MaxNumValues = 100;
	public const byte NumFaces = 8;
	public const int NumFeatures = 8;
	public const int NumEquip = 3;

	public string Name => this.RowId.ToString();

	public Genders Gender { get; private set; }
	public Race? Race { get; private set; }
	public Tribe? Tribe { get; private set; }

	public byte[] Voices { get; set; } = new byte[NumVoices];
	public FacialFeatureOptions[] FacialFeatureByFace { get; set; } = new FacialFeatureOptions[NumFaces];

	public Menu? Races { get; set; }
	public Menu? Genders { get; set; }
	public Menu? Ages { get; set; }
	public Menu? Heights { get; set; }
	public Menu? Tribes { get; set; }
	public Menu? Faces { get; set; }
	public Menu? Hairs { get; set; }
	public Menu? HighlightTypes { get; set; }
	public Menu? SkinTones { get; set; }
	public Menu? EyeColors { get; set; }
	public Menu? HairTones { get; set; }
	public Menu? Highlights { get; set; }
	public Menu? FacialFeatures { get; set; }
	public Menu? FacialFeatureColors { get; set; }
	public Menu? Eyebrows { get; set; }
	public Menu? Heterochromia { get; set; }
	public Menu? EyeShapes { get; set; }
	public Menu? Noses { get; set; }
	public Menu? Jaws { get; set; }
	public Menu? Mouths { get; set; }
	public Menu? LipsToneFurPatterns { get; set; }
	public Menu? EarMuscleTailSizes { get; set; }
	public Menu? TailEarsTypes { get; set; }
	public Menu? Busts { get; set; }
	public Menu? FacePaints { get; set; }
	public Menu? FacePaintColors { get; set; }

	public override void PopulateData(RowParser parser, LuminaData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Race = GameDataService.GetRow<Race>(parser.ReadColumn<int>(0));
		this.Tribe = GameDataService.GetRow<Tribe>(parser.ReadColumn<int>(1));
		this.Gender = (Genders)parser.ReadColumn<sbyte>(2);

		if (this.Race == null || this.Tribe == null)
			return;

		for (int i = 0; i < NumMenus; i++)
		{
			uint index = parser.ReadColumn<uint>(3 + (6 * NumMenus) + i);

			Menu menu = new();
			menu.Id = parser.ReadColumn<uint>(3 + (0 * NumMenus) + i);
			menu.InitVal = parser.ReadColumn<byte>(3 + (1 * NumMenus) + i);
			menu.Type = (Menu.Types)parser.ReadColumn<byte>(3 + (2 * NumMenus) + i);
			menu.NumOptions = parser.ReadColumn<byte>(3 + (3 * NumMenus) + i);
			menu.LookAt = parser.ReadColumn<byte>(3 + (4 * NumMenus) + i);
			menu.Mask = parser.ReadColumn<uint>(3 + (5 * NumMenus) + i);
			menu.CustomizationIndex = parser.ReadColumn<uint>(3 + (6 * NumMenus) + i);
			menu.Min = parser.ReadColumn<byte>(2999 + i);
			menu.Max = (byte)(parser.ReadColumn<byte>(87 + i) - 1 + menu.Min);

			// Hairstyles are MakeCustomize
			if (index == 6 || index == 24)
			{
				CharaMakeCustomizeSheet.Features featureType = index == 6 ? CharaMakeCustomizeSheet.Features.Hair : CharaMakeCustomizeSheet.Features.FacePaint;
				List<CharaMakeCustomize>? customize = GameDataService.CharaMakeCustomizes?.GetFeatureOptions(featureType, this.Tribe, this.Gender);

				if (customize != null)
				{
					menu.NumOptions = (byte)customize.Count;
					menu.AllOptions = new Menu.Option[menu.NumOptions];
					for (byte j = 0; j < menu.NumOptions; ++j)
					{
						menu.AllOptions[j] = new Menu.Option();
						menu.AllOptions[j].Value = customize[j].FeatureId;
						menu.AllOptions[j].Icon = customize[j].Icon;
						menu.AllOptions[j].Customize = customize[j];
						menu.AllOptions[j].Enabled = true;
					}
				}
			}
			else
			{
				// Colors are Human.cmp
				HumanCmp.Entry[]? colors = index switch
				{
					8 => HumanCmp.GetSkin(this.Tribe, this.Gender),
					9 => HumanCmp.GetEyeColors(),
					10 => HumanCmp.GetHair(this.Tribe, this.Gender),
					11 => HumanCmp.GetHairHighlights(),
					13 => HumanCmp.GetLimbalColors(), // what about for non au-ra? hmmm
					20 => HumanCmp.GetLipColors(),
					25 => HumanCmp.GetFacePaintColor(),
					_ => null,
				};

				if (colors != null)
					menu.NumOptions = (byte)colors.Length;

				menu.AllOptions = new Menu.Option[menu.NumOptions];
				for (byte j = 0; j < menu.NumOptions; ++j)
				{
					menu.AllOptions[j] = new Menu.Option();
					menu.AllOptions[j].Value = (byte)(menu.Min + j);
					menu.AllOptions[j].Enabled = menu.AllOptions[j].Value >= menu.Min && menu.AllOptions[j].Value <= menu.Max;

					if (menu.Type == Menu.Types.ColorPicker || menu.Type == Menu.Types.DoubleColorPicker)
					{
						////if (colors == null || colors.Length != menu.NumOptions)
						////	throw new Exception("No color or colors where wrong count for menu type");

						if (colors != null && j < colors.Length)
						{
							menu.AllOptions[j].Color = colors[j];
						}
					}
					else
					{
						menu.AllOptions[j].Icon = new ImageReference(parser.ReadColumn<uint>(3 + ((7 + j) * NumMenus) + i));
					}
				}
			}

			/*option.Graphic = new byte[NumGraphics];
			for (var j = 0; j < NumGraphics; ++j)
			{
				option.Graphic[j] = parser.ReadColumn<byte>(3 + ((MaxNumValues + 7 + j) * NumOptions) + i);
			}*/

			switch (index)
			{
				case 0: this.Races = menu; break;
				case 1: this.Genders = menu; break;
				case 2: this.Ages = menu; break;
				case 3: this.Heights = menu; break;
				case 4: this.Tribes = menu; break;
				case 5: this.Faces = menu; break;
				case 6: this.Hairs = menu; break;
				case 7: this.HighlightTypes = menu; break;
				case 8: this.SkinTones = menu; break;
				case 9: this.EyeColors = menu; break;
				case 10: this.HairTones = menu; break;
				case 11: this.Highlights = menu; break;
				case 12: this.FacialFeatures = menu; break;
				case 13: this.FacialFeatureColors = menu; break;
				case 14: this.Eyebrows = menu; break;
				case 15: this.Heterochromia = menu; break;
				case 16: this.EyeShapes = menu; break;
				case 17: this.Noses = menu; break;
				case 18: this.Jaws = menu; break;
				case 19: this.Mouths = menu; break;
				case 20: this.LipsToneFurPatterns = menu; break;
				case 21: this.EarMuscleTailSizes = menu; break;
				case 22: this.TailEarsTypes = menu; break;
				case 23: this.Busts = menu; break;
				case 24: this.FacePaints = menu; break;
				case 25: this.FacePaintColors = menu; break;
			}
		}

		for (var i = 0; i < NumVoices; ++i)
		{
			this.Voices[i] = parser.ReadColumn<byte>(3 + ((MaxNumValues + 7 + NumGraphics) * NumMenus) + i);
		}

		for (byte i = 0; i < NumFaces; ++i)
		{
			this.FacialFeatureByFace[i] = new(i);
			for (byte j = 0; j < NumFeatures - 1; ++j)
			{
				FacialFeatureOptions.Option option = new();
				option.Icon = new(parser.ReadColumn<int>(3 + ((MaxNumValues + 7 + NumGraphics) * NumMenus) + NumVoices + (j * NumFaces) + i));
				this.FacialFeatureByFace[i].Options[j] = option;
			}

			this.FacialFeatureByFace[i].Options[0].Value = Customize.FacialFeatures.First;
			this.FacialFeatureByFace[i].Options[1].Value = Customize.FacialFeatures.Second;
			this.FacialFeatureByFace[i].Options[2].Value = Customize.FacialFeatures.Third;
			this.FacialFeatureByFace[i].Options[3].Value = Customize.FacialFeatures.Fourth;
			this.FacialFeatureByFace[i].Options[4].Value = Customize.FacialFeatures.Fifth;
			this.FacialFeatureByFace[i].Options[5].Value = Customize.FacialFeatures.Sixth;
			this.FacialFeatureByFace[i].Options[6].Value = Customize.FacialFeatures.Seventh;

			FacialFeatureOptions.Option legacyTattooOption = new();
			////legacyTattooOption.Icon = // hmmm
			this.FacialFeatureByFace[i].Options[7] = legacyTattooOption;
			this.FacialFeatureByFace[i].Options[7].Value = Customize.FacialFeatures.LegacyTattoo;
		}

		/*for (var i = 0; i < NumEquip; ++i)
		{
			Equip[i] = new CharaMakeType.CharaMakeTypeUnkData3347Obj()
			{
				Helmet = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 0),
				Top = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 1),
				Gloves = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 2),
				Legs = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 3),
				Shoes = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 4),
				Weapon = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 5),
				SubWeapon = parser.ReadColumn<ulong>(3 + (MaxNumValues + 7 + NumGraphics) * NumMenus + NumVoices + NumFaces * NumFeatures + i * 7 + 6),
			};
		}*/
	}

	public FacialFeatureOptions? GetFacialFeatures(uint faceId)
	{
		if (faceId <= 0 || faceId >= this.FacialFeatureByFace.Length)
			return null;

		return this.FacialFeatureByFace[faceId - 1];
	}

	public class Menu
	{
		public enum Types
		{
			ListSelector = 0,
			IconSelector = 1,
			ColorPicker = 2,
			DoubleColorPicker = 3,
			MultiIconSelector = 4,
			Percentage = 5,
		}

		public string? Name => GameDataService.GetRow<Lobby>(this.Id)?.Text;

		public int OptionIndex { get; set; }
		public uint Id { get; set; }
		public byte InitVal { get; set; }
		public Types Type { get; set; }
		public byte NumOptions { get; set; }
		public byte LookAt { get; set; }
		public uint Mask { get; set; }
		public uint CustomizationIndex { get; set; }
		public byte Min { get; set; }
		public byte Max { get; set; }
		public Option[]? AllOptions { get; set; }

		public Option[] Options
		{
			get
			{
				List<Option> options = new();

				if (this.AllOptions != null)
				{
					foreach (var option in this.AllOptions)
					{
						if (!option.Enabled)
							continue;

						options.Add(option);
					}
				}

				return options.ToArray();
			}
		}

		public byte[]? Values
		{
			get
			{
				if (this.AllOptions == null)
					return null;

				List<byte> values = new();
				foreach (Option op in this.AllOptions)
				{
					if (!op.Enabled)
						continue;

					values.Add(op.Value);
				}

				return values.ToArray();
			}
		}

		public Option? GetOption(byte value)
		{
			if (this.AllOptions == null)
				return null;

			foreach (Option op in this.AllOptions)
			{
				if (op.Value == value)
				{
					return op;
				}
			}

			return null;
		}

		public class Option
		{
			public string? Name { get; set; }
			public byte Value { get; set; }
			public ImageReference? Icon { get; set; }
			public HumanCmp.Entry Color { get; set; }
			public CharaMakeCustomize? Customize { get; set; }
			public bool Enabled { get; set; }
		}
	}

	public class FacialFeatureOptions
	{
		public FacialFeatureOptions(byte face)
		{
			this.Face = face;
		}

		public byte Face { get; init; }
		public Option[] Options { get; init; } = new Option[NumFeatures];

		public override string ToString()
		{
			return $"Facial Feature Options for face {this.Face}";
		}

		public class Option
		{
			public Structs.Customize.FacialFeatures Value { get; set; }
			public ImageReference? Icon { get; set; }
			public bool Enabled => true;
		}
	}
}
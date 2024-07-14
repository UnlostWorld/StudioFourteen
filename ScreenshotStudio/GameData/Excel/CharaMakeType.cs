//// Glamourer
//// https://github.com/Ottermandias/Glamourer/tree/main/Glamourer/GameData/CharaMakeParams.cs
//// Brio
//// https://github.com/Etheirys/Brio/blob/main/Brio/Resources/Sheets/BrioCharaMakeType.cs

namespace ScreenshotStudio.GameData.Excel;

using Anamnesis.Actor.Utilities;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.GameData.Sheets;
using ScreenshotStudio.Structs;
using System.Collections.Generic;
using System.Security.AccessControl;
using LuminaData = Lumina.GameData;

[Sheet("CharaMakeType", 0x80d7db6d)]
public class CharaMakeType : StudioExcelRow
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
			menu.Race = this.Race;
			menu.Tribe = this.Tribe;
			menu.Gender = this.Gender;
			menu.Id = parser.ReadColumn<uint>(3 + (0 * NumMenus) + i);
			menu.InitVal = parser.ReadColumn<byte>(3 + (1 * NumMenus) + i);
			menu.Type = (Menu.Types)parser.ReadColumn<byte>(3 + (2 * NumMenus) + i);
			menu.NumOptions = parser.ReadColumn<byte>(3 + (3 * NumMenus) + i);
			menu.LookAt = parser.ReadColumn<byte>(3 + (4 * NumMenus) + i);
			menu.Mask = parser.ReadColumn<uint>(3 + (5 * NumMenus) + i);
			menu.CustomizationIndex = (CustomizeIndex)parser.ReadColumn<uint>(3 + (6 * NumMenus) + i);
			menu.Min = parser.ReadColumn<byte>(2999 + i);
			menu.Max = (byte)(parser.ReadColumn<byte>(87 + i) - 1 + menu.Min);

			menu.Icons = new ImageReference[menu.NumOptions];
			for (byte j = 0; j < menu.NumOptions; ++j)
			{
				menu.Icons[j] = new ImageReference(parser.ReadColumn<uint>(3 + ((7 + j) * NumMenus) + i));
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

			this.FacialFeatureByFace[i].Options[0].Value = CustomizeDataExtensions.FacialFeatures.First;
			this.FacialFeatureByFace[i].Options[1].Value = CustomizeDataExtensions.FacialFeatures.Second;
			this.FacialFeatureByFace[i].Options[2].Value = CustomizeDataExtensions.FacialFeatures.Third;
			this.FacialFeatureByFace[i].Options[3].Value = CustomizeDataExtensions.FacialFeatures.Fourth;
			this.FacialFeatureByFace[i].Options[4].Value = CustomizeDataExtensions.FacialFeatures.Fifth;
			this.FacialFeatureByFace[i].Options[5].Value = CustomizeDataExtensions.FacialFeatures.Sixth;
			this.FacialFeatureByFace[i].Options[6].Value = CustomizeDataExtensions.FacialFeatures.Seventh;

			FacialFeatureOptions.Option legacyTattooOption = new();
			////legacyTattooOption.Icon = // hmmm
			this.FacialFeatureByFace[i].Options[7] = legacyTattooOption;
			this.FacialFeatureByFace[i].Options[7].Value = CustomizeDataExtensions.FacialFeatures.LegacyTattoo;
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
		public Race? Race { get; set; }
		public Tribe? Tribe { get; set; }
		public Genders Gender { get; set; }

		public int OptionIndex { get; set; }
		public uint Id { get; set; }
		public byte InitVal { get; set; }
		public Types Type { get; set; }
		public byte NumOptions { get; set; }
		public byte LookAt { get; set; }
		public uint Mask { get; set; }
		public CustomizeIndex CustomizationIndex { get; set; }
		public byte Min { get; set; }
		public byte Max { get; set; }

		public ImageReference[]? Icons { get; set; }

		public byte[]? Values
		{
			get
			{
				List<byte> values = new();
				for (byte i = this.Min; i <= this.Max; i++)
				{
					values.Add(i);
				}

				return values.ToArray();
			}
		}
	}

	public class FacialFeatureOptions(byte face)
	{
		public byte Face { get; init; } = face;
		public Option[] Options { get; init; } = new Option[NumFeatures];

		public override string ToString()
		{
			return $"Facial Feature Options for face {this.Face}";
		}

		public class Option
		{
			public CustomizeDataExtensions.FacialFeatures Value { get; set; }
			public ImageReference? Icon { get; set; }
			public bool Enabled => true;
		}
	}
}
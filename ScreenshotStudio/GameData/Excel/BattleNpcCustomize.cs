// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina;
using Lumina.Data;
using Lumina.Excel;

[Sheet("BNpcCustomize", 0x18f060d4)]
public class BattleNpcCustomize : ExcelRow
{
	public int FacePaintColor { get; private set; }
	public int FacePaint { get; private set; }
	public int ExtraFeature2OrBust { get; private set; }
	public int ExtraFeature1 { get; private set; }
	public Race? Race { get; private set; }
	public int Gender { get; private set; }
	public int BodyType { get; private set; }
	public int Height { get; private set; }
	public Tribe? Tribe { get; private set; }
	public int Face { get; private set; }
	public int HairStyle { get; private set; }
	public bool EnableHairHighlight { get; private set; }
	public int SkinColor { get; private set; }
	public int EyeHeterochromia { get; private set; }
	public int HairHighlightColor { get; private set; }
	public int FacialFeature { get; private set; }
	public int FacialFeatureColor { get; private set; }
	public int Eyebrows { get; private set; }
	public int EyeColor { get; private set; }
	public int EyeShape { get; private set; }
	public int Nose { get; private set; }
	public int Jaw { get; private set; }
	public int Mouth { get; private set; }
	public int LipColor { get; private set; }
	public int BustOrTone1 { get; private set; }
	public int HairColor { get; private set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Race = parser.ReadRowReference<byte, Race>(0);
		this.Gender = parser.ReadColumn<byte>(1);
		this.BodyType = parser.ReadColumn<byte>(2);
		this.Height = parser.ReadColumn<byte>(3);
		this.Tribe = parser.ReadRowReference<byte, Tribe>(4);
		this.Face = parser.ReadColumn<byte>(5);
		this.HairStyle = parser.ReadColumn<byte>(6);
		this.EnableHairHighlight = parser.ReadColumn<byte>(7) > 1;
		this.SkinColor = parser.ReadColumn<byte>(8);
		this.EyeHeterochromia = parser.ReadColumn<byte>(9);
		this.HairColor = parser.ReadColumn<byte>(10);
		this.HairHighlightColor = parser.ReadColumn<byte>(11);
		this.FacialFeature = parser.ReadColumn<byte>(12);
		this.FacialFeatureColor = parser.ReadColumn<byte>(13);
		this.Eyebrows = parser.ReadColumn<byte>(14);
		this.EyeColor = parser.ReadColumn<byte>(15);
		this.EyeShape = parser.ReadColumn<byte>(16);
		this.Nose = parser.ReadColumn<byte>(17);
		this.Jaw = parser.ReadColumn<byte>(18);
		this.Mouth = parser.ReadColumn<byte>(19);
		this.LipColor = parser.ReadColumn<byte>(20);
		this.BustOrTone1 = parser.ReadColumn<byte>(21);
		this.ExtraFeature1 = parser.ReadColumn<byte>(22);
		this.ExtraFeature2OrBust = parser.ReadColumn<byte>(23);
		this.FacePaint = parser.ReadColumn<byte>(24);
		this.FacePaintColor = parser.ReadColumn<byte>(25);
	}
}
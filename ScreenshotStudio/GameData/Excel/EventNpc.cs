// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.GameData.Sheets;

[Sheet("ENpcBase", 0x927347d8)]
public class EventNpc : LibraryExcelRow
{
	public string? Name { get; protected set; }

	// Customize
	public float Scale { get; protected set; } = 1.0f;
	public ModelChara? ModelChara { get; protected set; }
	public int FacePaintColor { get; protected set; }
	public int FacePaint { get; protected set; }
	public int ExtraFeature2OrBust { get; protected set; }
	public int ExtraFeature1 { get; protected set; }
	public Race? Race { get; protected set; }
	public int Gender { get; protected set; }
	public int BodyType { get; protected set; }
	public int Height { get; protected set; }
	public Tribe? Tribe { get; protected set; }
	public int Face { get; protected set; }
	public int HairStyle { get; protected set; }
	public bool EnableHairHighlight { get; protected set; } = false;
	public int SkinColor { get; protected set; }
	public int EyeHeterochromia { get; protected set; }
	public int HairHighlightColor { get; protected set; }
	public int FacialFeature { get; protected set; }
	public int FacialFeatureColor { get; protected set; }
	public int Eyebrows { get; protected set; }
	public int EyeColor { get; protected set; }
	public int EyeShape { get; protected set; }
	public int Nose { get; protected set; }
	public int Jaw { get; protected set; }
	public int Mouth { get; protected set; }
	public int LipColor { get; protected set; }
	public int BustOrTone1 { get; protected set; }
	public int HairColor { get; protected set; }

	// Gear
	public Item? MainHand { get; protected set; }
	public Stain? DyeMainHand { get; protected set; }
	public Item? OffHand { get; protected set; }
	public Stain? DyeOffHand { get; protected set; }
	public Item? Head { get; protected set; }
	public Stain? DyeHead { get; protected set; }
	public Item? Body { get; protected set; }
	public Stain? DyeBody { get; protected set; }
	public Item? Legs { get; protected set; }
	public Stain? DyeLegs { get; protected set; }
	public Item? Feet { get; protected set; }
	public Stain? DyeFeet { get; protected set; }
	public Item? Hands { get; protected set; }
	public Stain? DyeHands { get; protected set; }
	public Item? Wrists { get; protected set; }
	public Stain? DyeWrists { get; protected set; }
	public Item? Neck { get; protected set; }
	public Stain? DyeNeck { get; protected set; }
	public Item? Ears { get; protected set; }
	public Stain? DyeEars { get; protected set; }
	public Item? LeftRing { get; protected set; }
	public Stain? DyeLeftRing { get; protected set; }
	public Item? RightRing { get; protected set; }
	public Stain? DyeRightRing { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = $"Event NPC #{this.RowId}";

		// Customize
		this.Scale = parser.ReadColumn<float>(34);
		this.ModelChara = parser.ReadRowReference<ushort, ModelChara>(35);
		this.Race = parser.ReadRowReference<byte, Race>(36, 1);
		this.Gender = parser.ReadColumn<byte>(37);
		this.BodyType = parser.ReadColumn<byte>(38);
		this.Height = parser.ReadColumn<byte>(39);
		this.Tribe = parser.ReadRowReference<byte, Tribe>(40, 1);
		this.Face = parser.ReadColumn<byte>(41);
		this.HairStyle = parser.ReadColumn<byte>(42);
		this.EnableHairHighlight = parser.ReadColumn<byte>(43) > 1;
		this.SkinColor = parser.ReadColumn<byte>(44);
		this.EyeHeterochromia = parser.ReadColumn<byte>(45);
		this.HairColor = parser.ReadColumn<byte>(46);
		this.HairHighlightColor = parser.ReadColumn<byte>(47);
		this.FacialFeature = parser.ReadColumn<byte>(48);
		this.FacialFeatureColor = parser.ReadColumn<byte>(49);
		this.Eyebrows = parser.ReadColumn<byte>(50);
		this.EyeColor = parser.ReadColumn<byte>(51);
		this.EyeShape = parser.ReadColumn<byte>(52);
		this.Nose = parser.ReadColumn<byte>(53);
		this.Jaw = parser.ReadColumn<byte>(54);
		this.Mouth = parser.ReadColumn<byte>(55);
		this.LipColor = parser.ReadColumn<byte>(56);
		this.BustOrTone1 = parser.ReadColumn<byte>(57);
		this.ExtraFeature2OrBust = parser.ReadColumn<byte>(58);
		this.ExtraFeature1 = parser.ReadColumn<byte>(59);
		this.FacePaint = parser.ReadColumn<byte>(60);
		this.FacePaintColor = parser.ReadColumn<byte>(61);

		NpcEquip? npcEquip = parser.ReadRowReference<ushort, NpcEquip>(63);
		if (npcEquip?.RowId == 175)
			npcEquip = null;

		// Gear
		this.MainHand = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(65));
		this.DyeMainHand = parser.ReadRowReference<byte, Stain>(66);
		this.OffHand = GameDataService.Items?.Find(ItemSlots.OffHand, parser.ReadColumn<ulong>(67));
		this.DyeOffHand = parser.ReadRowReference<byte, Stain>(68);
		this.Head = this.GetItem(ItemSlots.Head, parser.ReadColumn<uint>(69), npcEquip?.Head);
		this.DyeHead = parser.ReadRowReference<byte, Stain>(70);
		this.Body = this.GetItem(ItemSlots.Chest, parser.ReadColumn<uint>(72), npcEquip?.Body);
		this.DyeBody = parser.ReadRowReference<byte, Stain>(73);
		this.Hands = this.GetItem(ItemSlots.Hands, parser.ReadColumn<uint>(74), npcEquip?.Hands);
		this.DyeHands = parser.ReadRowReference<byte, Stain>(75);
		this.Legs = this.GetItem(ItemSlots.Legs, parser.ReadColumn<uint>(76), npcEquip?.Legs);
		this.DyeLegs = parser.ReadRowReference<byte, Stain>(77);
		this.Feet = this.GetItem(ItemSlots.Feet, parser.ReadColumn<uint>(78), npcEquip?.Feet);
		this.DyeFeet = parser.ReadRowReference<byte, Stain>(79);
		this.Ears = this.GetItem(ItemSlots.Earring, parser.ReadColumn<uint>(80), npcEquip?.Ears);
		this.DyeEars = parser.ReadRowReference<byte, Stain>(81);
		this.Neck = this.GetItem(ItemSlots.Necklace, parser.ReadColumn<uint>(82), npcEquip?.Neck);
		this.DyeNeck = parser.ReadRowReference<byte, Stain>(83);
		this.Wrists = this.GetItem(ItemSlots.Bracelet, parser.ReadColumn<uint>(84), npcEquip?.Wrists);
		this.DyeWrists = parser.ReadRowReference<byte, Stain>(85);
		this.LeftRing = this.GetItem(ItemSlots.RingLeft, parser.ReadColumn<uint>(86), npcEquip?.LeftRing);
		this.DyeLeftRing = parser.ReadRowReference<byte, Stain>(87);
		this.RightRing = this.GetItem(ItemSlots.RingRight, parser.ReadColumn<uint>(88), npcEquip?.RightRing);
		this.DyeRightRing = parser.ReadRowReference<byte, Stain>(89);
	}

	// This is a little funky, but as some point (Heavenswars?) SQEX changed where NPC's stored their equipment
	// so we need to check both the old data and the new for valid values.
	protected Item? GetItem(ItemSlots slot, uint baseVal, Item? equipVal)
	{
		if (equipVal != null && equipVal != ItemsSheet.None)
			return equipVal;

		return GameDataService.Items?.Find(slot, baseVal);
	}
}

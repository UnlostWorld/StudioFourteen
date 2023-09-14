// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina;
using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.GameData.Sheets;

[Sheet("NpcEquip", 0xe91c87ba)]
public class NpcEquip : StudioExcelRow
{
	public Item? MainHand { get; protected set; } = null;
	public Stain? DyeMainHand { get; protected set; } = null;
	public Item? OffHand { get; protected set; } = null;
	public Stain? DyeOffHand { get; protected set; } = null;
	public Item? Head { get; protected set; } = null;
	public Stain? DyeHead { get; protected set; } = null;
	public Item? Body { get; protected set; } = null;
	public Stain? DyeBody { get; protected set; } = null;
	public Item? Legs { get; protected set; } = null;
	public Stain? DyeLegs { get; protected set; } = null;
	public Item? Feet { get; protected set; } = null;
	public Stain? DyeFeet { get; protected set; } = null;
	public Item? Hands { get; protected set; } = null;
	public Stain? DyeHands { get; protected set; } = null;
	public Item? Wrists { get; protected set; } = null;
	public Stain? DyeWrists { get; protected set; } = null;
	public Item? Neck { get; protected set; } = null;
	public Stain? DyeNeck { get; protected set; } = null;
	public Item? Ears { get; protected set; } = null;
	public Stain? DyeEars { get; protected set; } = null;
	public Item? LeftRing { get; protected set; } = null;
	public Stain? DyeLeftRing { get; protected set; } = null;
	public Item? RightRing { get; protected set; } = null;
	public Stain? DyeRightRing { get; protected set; } = null;
	public bool Visor { get; protected set; } = true;

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.MainHand = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(0));
		this.DyeMainHand = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(1));
		this.OffHand = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(2));
		this.DyeOffHand = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(3));
		this.Head = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(4));
		this.DyeHead = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(5));
		this.Visor = parser.ReadColumn<bool>(6);
		this.Body = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(7));
		this.DyeBody = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(8));
		this.Hands = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(9));
		this.DyeHands = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(10));
		this.Legs = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(11));
		this.DyeLegs = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(12));
		this.Feet = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(13));
		this.DyeFeet = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(14));
		this.Ears = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(15));
		this.DyeEars = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(16));
		this.Neck = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(17));
		this.DyeNeck = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(18));
		this.Wrists = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(19));
		this.DyeWrists = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(20));
		this.LeftRing = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(21));
		this.DyeLeftRing = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(22));
		this.RightRing = GameDataService.Items?.Find(ItemSlots.MainHand, parser.ReadColumn<ulong>(23));
		this.DyeRightRing = parser.ReadRowReference<byte, Stain>(parser.ReadColumn<byte>(24));
	}
}
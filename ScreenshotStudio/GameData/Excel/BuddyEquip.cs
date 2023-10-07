namespace ScreenshotStudio.GameData.Excel;

using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using ScreenshotStudio.GameData.Sheets;

[Sheet("BuddyEquip", 0xb429792a)]
public class BuddyEquip : StudioExcelRow
{
	public BuddyItem? Head { get; protected set; }
	public BuddyItem? Body { get; protected set; }
	public BuddyItem? Feet { get; protected set; }

	public override void PopulateData(RowParser parser, GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		string name = parser.ReadColumn<SeString>(8) ?? string.Empty;

		int h = parser.ReadColumn<int>(9);
		ushort headBase = (ushort)h;
		ushort headVariant = (ushort)(h >> 16);
		ushort headIcon = parser.ReadColumn<ushort>(13);
		if (headBase != 0 || headVariant != 0)
			this.Head = new(name, ItemSlots.Head, headBase, headVariant, headIcon);

		int b = parser.ReadColumn<int>(10);
		ushort bodyBase = (ushort)b;
		ushort bodyVariant = (ushort)(b >> 16);
		ushort bodyIcon = parser.ReadColumn<ushort>(14);
		if (bodyBase != 0 || bodyVariant != 0)
			this.Body = new(name, ItemSlots.Chest, bodyBase, bodyVariant, bodyIcon);

		int l = parser.ReadColumn<int>(11);
		ushort legsBase = (ushort)l;
		ushort legsVariant = (ushort)(l >> 16);
		ushort legsIcon = parser.ReadColumn<ushort>(15);
		if (legsBase != 0 || legsVariant != 0)
		{
			this.Feet = new(name, ItemSlots.Feet, legsBase, legsVariant, legsIcon);
		}
	}
}
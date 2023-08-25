// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;

[Sheet("Mount", 0x33b2e4b2)]
public class Mount : LibraryExcelRow
{
	public string? Name { get; protected set; }
	public ModelChara? ModelChara { get; protected set; }
	public MountCustomize? MountCustomize { get; protected set; }
	public Item? Head { get; protected set; }
	public Item? Body { get; protected set; }
	public Item? Legs { get; protected set; }
	public Item? Feet { get; protected set; }
	public ImageReference? Icon { get; private set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.ModelChara = parser.ReadRowReference<int, ModelChara>(8);
		this.MountCustomize = parser.ReadRowReference<byte, MountCustomize>(16);

		this.Head = GameDataService.BuddyEquips?.Find(ItemSlots.Head, parser.ReadColumn<int>(25));
		this.Body = GameDataService.BuddyEquips?.Find(ItemSlots.Head, parser.ReadColumn<int>(26));
		this.Legs = GameDataService.BuddyEquips?.Find(ItemSlots.Head, parser.ReadColumn<int>(27));
		this.Feet = GameDataService.BuddyEquips?.Find(ItemSlots.Head, parser.ReadColumn<int>(28));
		this.Icon = parser.ReadImageReference<ushort>(30);
	}
}

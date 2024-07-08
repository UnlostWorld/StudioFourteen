//// Lumina
//// https://github.com/NotAdam/Lumina.Excel/blob/master/src/Lumina.Excel/GeneratedSheets2/ClassJob.cs

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("ClassJob", 0xe62cb7ae)]
public partial class ClassJob : StudioExcelRow
{
	public enum ClassJobRows
	{
		None,

		Gladiator,
		Pugilist,
		Marauder,
		Lancer,
		Archer,
		Conjurer,
		Thaumaturge,
		Carpenter,
		Blacksmith,
		Armorer,
		Goldsmith,
		Leatherworker,
		Weaver,
		Alchemist,
		Culinarian,
		Miner,
		Botanist,
		Fisher,
		Paladin,
		Monk,
		Warrior,
		Dragoon,
		Bard,
		WhiteMage,
		BlackMage,
		Arcanist,
		Summoner,
		Scholar,
		Rogue,
		Ninja,
		Machinist,
		DarkKnight,
		Astrologian,
		Samurai,
		RedMage,
		BlueMage,
		Gunbreaker,
		Dancer,
		Reaper,
		Sage,
		Viper,
		Pictomancer,

		Count,
	}

	public enum Roles : byte
	{
		None,
		Tank,
		MeleeDamage,
		RangedDamage,
		Healer,
	}

	public string? Name { get; set; }
	public string? Abbreviation { get; set; }
	public ClassJobCategory? ClassJobCategory { get; set; }
	public byte ParentRow { get; set; }
	public string? NameEnglish { get; set; }
	public Roles Role { get; set; }

	public bool IsClass => this.ParentRow == (byte)this.RowId;
	public bool IsJob => !this.IsClass;
	public ImageReference? Icon { get; protected set; }
	public ImageReference? SmallIcon { get; protected set; }

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadStringOffset(0);
		this.Abbreviation = parser.ReadStringOffset(4);
		this.NameEnglish = parser.ReadStringOffset(16);
		this.ClassJobCategory = parser.ReadRowReferenceOffset<byte, ClassJobCategory>(86);
		this.ParentRow = parser.ReadOffset<byte>(92);
		this.Role = (Roles)parser.ReadOffset<byte>(93);

		this.Icon = new ImageReference(062100 + this.RowId);
		this.SmallIcon = new ImageReference(062225 + this.RowId);
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		if (this.NameEnglish != null)
			tags.Add(this.NameEnglish).WithAlias(this.Abbreviation);

		if (this.Role != Roles.None)
			tags.Add(this.Role.ToString());

		if (this.IsClass)
			tags.Add("Class");

		if (this.IsJob)
			tags.Add("Job");

		if (this.RowId == (uint)ClassJobRows.BlueMage)
		{
			tags.Add("Limited");
		}

		return tags;
	}
}
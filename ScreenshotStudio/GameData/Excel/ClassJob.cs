// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using ScreenshotStudio.Tags;

[Sheet("ClassJob", columnHash: 0x16808bcd)]
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
	public string? Unknown2 { get; set; }
	public ClassJobCategory? ClassJobCategory { get; set; }
	public byte ParentRow { get; set; }
	public string? NameEnglish { get; set; }
	public Roles Role { get; set; }

	public bool IsClass => this.ParentRow == (byte)this.RowId;
	public bool IsJob => !this.IsClass;

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Name = parser.ReadString(0);
		this.Abbreviation = parser.ReadString(1);
		this.Unknown2 = parser.ReadString(2);
		this.ClassJobCategory = parser.ReadRowReference<byte, ClassJobCategory>(3);
		////this.JobIndex = parser.ReadColumn<byte>(7);
		////this.DohDolJobIndex = parser.ReadColumn<sbyte>(8);
		this.ParentRow = parser.ReadColumn<byte>(26); ////parser.ReadRowReference<byte, ClassJob>(26);
		this.NameEnglish = parser.ReadString(27);
		this.Role = (Roles)parser.ReadColumn<byte>(30);
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
namespace StudioFourteen.GameData.Excel;

using System;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using System.Collections.Generic;

using StudioFourteen.Tags;

[Sheet("Race", 0x3403807a)]
public class Race : LibraryExcelRow
{
	public enum RaceRows : byte
	{
		Hyur = 1,
		Elezen = 2,
		Lalafell = 3,
		Miqote = 4,
		Roegadyn = 5,
		AuRa = 6,
		Hrothgar = 7,
		Viera = 8,

		Count,
	}

	public RaceRows RowEnum => (RaceRows)this.RowId;

	public string Feminine { get; private set; } = string.Empty;
	public string Masculine { get; private set; } = string.Empty;

	public int RacialGearMasculineBodyId { get; private set; }
	public Item? RacialGearMasculineBody => GameDataService.GetRow<Item>(this.RacialGearMasculineBodyId);

	public int RacialGearMasculineHandsId { get; private set; }
	public Item? RacialGearMasculineHands => GameDataService.GetRow<Item>(this.RacialGearMasculineHandsId);

	public int RacialGearMasculineLegsId { get; private set; }
	public Item? RacialGearMasculineLegs => GameDataService.GetRow<Item>(this.RacialGearMasculineLegsId);

	public int RacialGearMasculineFeetId { get; private set; }
	public Item? RacialGearMasculineFeet => GameDataService.GetRow<Item>(this.RacialGearMasculineFeetId);

	public int RacialGearFeminineBodyId { get; private set; }
	public Item? RacialGearFeminineBody => GameDataService.GetRow<Item>(this.RacialGearFeminineBodyId);

	public int RacialGearFeminineHandsId { get; private set; }
	public Item? RacialGearFeminineHands => GameDataService.GetRow<Item>(this.RacialGearFeminineHandsId);

	public int RacialGearFeminineLegsId { get; private set; }
	public Item? RacialGearFeminineLegs => GameDataService.GetRow<Item>(this.RacialGearFeminineLegsId);

	public int RacialGearFeminineFeetId { get; private set; }
	public Item? RacialGearFeminineFeet => GameDataService.GetRow<Item>(this.RacialGearFeminineFeetId);

	// Customize options
	public List<Tribe?> Tribes { get; private set; } = new();
	public List<Genders> Genders { get; private set; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;

		this.Name = this.Masculine;

		this.RacialGearMasculineBodyId = parser.ReadColumn<int>(2);
		this.RacialGearMasculineHandsId = parser.ReadColumn<int>(3);
		this.RacialGearMasculineLegsId = parser.ReadColumn<int>(4);
		this.RacialGearMasculineFeetId = parser.ReadColumn<int>(5);
		this.RacialGearFeminineBodyId = parser.ReadColumn<int>(6);
		this.RacialGearFeminineHandsId = parser.ReadColumn<int>(7);
		this.RacialGearFeminineLegsId = parser.ReadColumn<int>(8);
		this.RacialGearFeminineFeetId = parser.ReadColumn<int>(9);

		this.Tribes = (RaceRows)this.RowId switch
		{
			RaceRows.Hyur => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Midlander),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Highlander),
			},

			RaceRows.Elezen => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Wildwood),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Duskwight),
			},

			RaceRows.Lalafell => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Plainsfolk),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Dunesfolk),
			},

			RaceRows.Miqote => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.SeekerOfTheSun),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.KeeperOfTheMoon),
			},

			RaceRows.Roegadyn => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.SeaWolf),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Hellsguard),
			},

			RaceRows.AuRa => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Raen),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Xaela),
			},

			RaceRows.Hrothgar => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Helions),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.TheLost),
			},

			RaceRows.Viera => new()
			{
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Rava),
				GameDataService.GetRow<Tribe>((byte)Tribe.TribeRows.Veena),
			},

			_ => new(),
		};

		this.Genders.Clear();
		this.Genders.Add(Excel.Genders.Masculine);
		this.Genders.Add(Excel.Genders.Feminine);
	}

	public bool Is(RaceRows raceRow)
	{
		return this.RowId == (uint)raceRow;
	}

	public TagCollection ToTags()
	{
		TagCollection tags = new();

		if (!string.IsNullOrEmpty(this.Feminine))
			tags.Add(this.Feminine);

		if (!string.IsNullOrEmpty(this.Masculine))
			tags.Add(this.Masculine);

		return tags;
	}

	/*public Item? GetRacialGear(CustomizeGenders gender, ItemSlots slot)
	{
		switch (slot)
		{
			case ItemSlots.Chest: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineBody : this.RacialGearFeminineBody;
			case ItemSlots.Hands: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineHands : this.RacialGearFeminineHands;
			case ItemSlots.Legs: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineLegs : this.RacialGearFeminineLegs;
			case ItemSlots.Feet: return gender == CustomizeGenders.Masculine ? this.RacialGearMasculineFeet : this.RacialGearFeminineFeet;
		}

		return null;
	}*/
}

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

	public Item? RacialGearMasculineBody { get; private set; }
	public Item? RacialGearMasculineHands { get; private set; }
	public Item? RacialGearMasculineLegs { get; private set; }
	public Item? RacialGearMasculineFeet { get; private set; }
	public Item? RacialGearFeminineBody { get; private set; }
	public Item? RacialGearFeminineHands { get; private set; }
	public Item? RacialGearFeminineLegs { get; private set; }
	public Item? RacialGearFeminineFeet { get; private set; }

	// Customize options
	public List<Tribe?> Tribes { get; private set; } = new();
	public List<Genders> Genders { get; private set; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;

		this.Name = this.Masculine;

		this.RacialGearMasculineBody = parser.ReadRowReference<int, Item>(2);
		this.RacialGearMasculineHands = parser.ReadRowReference<int, Item>(3);
		this.RacialGearMasculineLegs = parser.ReadRowReference<int, Item>(4);
		this.RacialGearMasculineFeet = parser.ReadRowReference<int, Item>(5);
		this.RacialGearFeminineBody = parser.ReadRowReference<int, Item>(6);
		this.RacialGearFeminineHands = parser.ReadRowReference<int, Item>(7);
		this.RacialGearFeminineLegs = parser.ReadRowReference<int, Item>(8);
		this.RacialGearFeminineFeet = parser.ReadRowReference<int, Item>(9);

		this.Tribes = (RaceRows)this.RowId switch
		{
			RaceRows.Hyur => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Midlander),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Highlander),
			},

			RaceRows.Elezen => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Wildwood),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Duskwight),
			},

			RaceRows.Lalafell => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Plainsfolk),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Dunesfolk),
			},

			RaceRows.Miqote => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.SeekerOfTheSun),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.KeeperOfTheMoon),
			},

			RaceRows.Roegadyn => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.SeaWolf),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Hellsguard),
			},

			RaceRows.AuRa => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Raen),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Xaela),
			},

			RaceRows.Hrothgar => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Helions),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.TheLost),
			},

			RaceRows.Viera => new()
			{
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Rava),
				gameData.GetRow<Tribe>((byte)Tribe.TribeRows.Veena),
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

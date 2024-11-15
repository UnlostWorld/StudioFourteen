namespace Lumina.Excel.Sheets;

using Lumina.Excel;
using StudioFourteen;
using StudioFourteen.GameData;
using StudioFourteen.Tags;
using System;

public static class RaceTribeExtensions
{
	public static Tribe[] GetTribes(this Race race)
	{
		ExcelSheet<Tribe>? tribeSheet = ServiceManager.Instance.GameData.GetSheet<Tribe>();
		if (tribeSheet == null)
			return [];

		return (RaceRows)race.RowId switch
		{
			RaceRows.Hyur =>
			[
				tribeSheet.GetRow(TribeRows.Midlander),
				tribeSheet.GetRow(TribeRows.Highlander),
			],

			RaceRows.Elezen =>
			[
				tribeSheet.GetRow(TribeRows.Wildwood),
				tribeSheet.GetRow(TribeRows.Duskwight),
			],

			RaceRows.Lalafell =>
			[
				tribeSheet.GetRow(TribeRows.Plainsfolk),
				tribeSheet.GetRow(TribeRows.Dunesfolk),
			],

			RaceRows.Miqote =>
			[
				tribeSheet.GetRow(TribeRows.SeekerOfTheSun),
				tribeSheet.GetRow(TribeRows.KeeperOfTheMoon),
			],

			RaceRows.Roegadyn =>
			[
				tribeSheet.GetRow(TribeRows.SeaWolf),
				tribeSheet.GetRow(TribeRows.Hellsguard),
			],

			RaceRows.AuRa =>
			[
				tribeSheet.GetRow(TribeRows.Raen),
				tribeSheet.GetRow(TribeRows.Xaela),
			],

			RaceRows.Hrothgar =>
			[
				tribeSheet.GetRow(TribeRows.Helions),
				tribeSheet.GetRow(TribeRows.TheLost),
			],

			RaceRows.Viera =>
			[
				tribeSheet.GetRow(TribeRows.Rava),
				tribeSheet.GetRow(TribeRows.Veena),
			],

			_ => throw new Exception($"Unrecognized race {race.RowId}"),
		};
	}

	public static Tag? GetTag(this Race race)
	{
		string tagName = race.Feminine.ExtractText();

		if (string.IsNullOrEmpty(tagName))
			return null;

		return Tag.Get(tagName);
	}

	public static TagCollection? ToTags(this Race race)
	{
		TagCollection tags = new();
		tags.Add(race.Feminine.ExtractText());
		tags.Add(race.Masculine.ExtractText());
		return tags;
	}

	public static TagCollection ToTags(this Tribe race)
	{
		TagCollection tags = new();
		tags.Add(race.Feminine.ExtractText());
		tags.Add(race.Masculine.ExtractText());
		return tags;
	}

	public static int GetTribeIndex(this Race race, Tribe tribe)
	{
		Tribe[] tribes = race.GetTribes();
		for (int i = 0; i < tribes.Length; i++)
		{
			if (tribes[i].RowId == tribe.RowId)
			{
				return i;
			}
		}

		return -1;
	}

	public static ModelTypes[] GetModelTypes(this Tribe tribe)
	{
		switch ((TribeRows)tribe.RowId)
		{
			case TribeRows.Midlander:
			case TribeRows.Wildwood:
			case TribeRows.Duskwight:
			case TribeRows.SeekerOfTheSun:
			case TribeRows.KeeperOfTheMoon:
			case TribeRows.Raen:
			case TribeRows.Xaela:
			{
				return
				[
					ModelTypes.Young,
					ModelTypes.Normal,
					ModelTypes.Old,
				];
			}

			case TribeRows.Highlander:
			case TribeRows.Plainsfolk:
			case TribeRows.Dunesfolk:
			case TribeRows.SeaWolf:
			case TribeRows.Hellsguard:
			case TribeRows.Helions:
			case TribeRows.TheLost:
			case TribeRows.Rava:
			case TribeRows.Veena:
			{
				return
				[
					ModelTypes.Normal,
				];
			}
		}

		throw new Exception($"Unrecognized tribe {tribe.RowId}");
	}
}

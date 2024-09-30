namespace ScreenshotStudio.GameData.Excel;

using Lumina.Data;
using Lumina.Excel;
using Lumina.Text;
using ScreenshotStudio.Tags;
using System.Collections.Generic;

[Sheet("Tribe", 0xe74759fb)]
public class Tribe : StudioExcelRow
{
	public enum TribeRows : byte
	{
		Midlander = 1,
		Highlander = 2,
		Wildwood = 3,
		Duskwight = 4,
		Plainsfolk = 5,
		Dunesfolk = 6,
		SeekerOfTheSun = 7,
		KeeperOfTheMoon = 8,
		SeaWolf = 9,
		Hellsguard = 10,
		Raen = 11,
		Xaela = 12,
		Helions = 13,
		TheLost = 14,
		Rava = 15,
		Veena = 16,
	}

	public string Name => this.Masculine;

	public string Feminine { get; private set; } = string.Empty;
	public string Masculine { get; private set; } = string.Empty;

	public List<ModelTypes> ModelTypes { get; private set; } = new();

	public override void PopulateData(RowParser parser, Lumina.GameData gameData, Language language)
	{
		base.PopulateData(parser, gameData, language);

		this.Masculine = parser.ReadColumn<SeString>(0) ?? string.Empty;
		this.Feminine = parser.ReadColumn<SeString>(1) ?? string.Empty;

		switch((TribeRows)this.RowId)
		{
			case TribeRows.Midlander:
			case TribeRows.Wildwood:
			case TribeRows.Duskwight:
			case TribeRows.SeekerOfTheSun:
			case TribeRows.KeeperOfTheMoon:
			case TribeRows.Raen:
			case TribeRows.Xaela:
			{
				this.ModelTypes.Add(Excel.ModelTypes.Young);
				this.ModelTypes.Add(Excel.ModelTypes.Normal);
				this.ModelTypes.Add(Excel.ModelTypes.Old);
				break;
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
				this.ModelTypes.Add(Excel.ModelTypes.Normal);
				break;
			}
		}
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
}

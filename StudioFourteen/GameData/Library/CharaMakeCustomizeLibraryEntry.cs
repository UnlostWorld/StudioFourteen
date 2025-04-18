// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.GameData.Library;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Extensions;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Threading.Tasks;

using HairMakeType = StudioFourteen.GameData.Sheets.HairMakeType;

public class CharaMakeCustomizeLibraryEntry : LibraryEntryBase
{
	public readonly Race Race;
	public readonly Tribe Tribe;
	public readonly Genders Gender;
	public readonly RowRef<CharaMakeCustomize> MakeCustomize;
	public readonly CustomizeIndex CustomizeIndex;

	public CharaMakeCustomizeLibraryEntry(
		SourceBase source,
		Race race,
		Tribe tribe,
		Genders gender,
		RowRef<CharaMakeCustomize> makeCustomize,
		CustomizeIndex customizeIndex)
		: base(source)
	{
		this.Race = race;
		this.Tribe = tribe;
		this.Gender = gender;
		this.MakeCustomize = makeCustomize;
		this.CustomizeIndex = customizeIndex;

		this.Tags.Add(race.ToTags());
		this.Tags.Add(tribe.ToTags());
		this.Tags.Add(gender.ToTags());
		this.Tags.Add(customizeIndex.ToTag());
	}

	public override string? SubTitle => $"#{this.MakeCustomize.Value.FeatureID}";
	public override object? Icon => new ImageReference(this.MakeCustomize.Value.Icon);
	public override IComparable DefaultSortValue => this.MakeCustomize.Value.FeatureID;

	public override string? Name
	{
		get
		{
			string? name = this.MakeCustomize.Value.HintItem.Value.Name.GetString();
			name = name?.Replace("Modern Aesthetics - ", string.Empty);
			name = name?.Replace("Modern Cosmetics - ", string.Empty);
			return name;
		}
	}

	public override string ToString() => $"#{this.MakeCustomize.RowId}";

	public async Task Apply(int objectTableIndex)
	{
		await TickService.GameTick();
		this.Services.CharacterAppearance.SetCustomizeValue(objectTableIndex, this.CustomizeIndex, this.MakeCustomize.Value.FeatureID, UpdateSource.Interface);
	}

	protected override string GetInternalId() => $"{this.MakeCustomize.RowId}";
}

public abstract class CharaMakeCustomizeLibrarySource : SourceBase
{
	public CharaMakeCustomizeLibraryEntry? Find(Race race, Tribe tribe, Genders gender, CustomizeIndex customizeIndex, byte featureId)
	{
		foreach (CharaMakeCustomizeLibraryEntry entry in this.AllEntries)
		{
			if (entry.Race.RowId == race.RowId
				&& entry.Tribe.RowId == tribe.RowId
				&& entry.Gender == gender
				&& entry.CustomizeIndex == customizeIndex
				&& entry.MakeCustomize.Value.FeatureID == featureId)
			{
				return entry;
			}
		}

		return null;
	}
}

public class HairLibrarySource : CharaMakeCustomizeLibrarySource
{
	public override string? Name => "Hair";

	protected override string GetInternalId() => $"Hair";

	protected override void Scan()
	{
		ExcelSheet<HairMakeType>? hairMakeTypeSheet = this.Services.GameData.GetSheet<HairMakeType>();

		if (hairMakeTypeSheet == null)
			return;

		foreach (HairMakeType hairMakeType in hairMakeTypeSheet)
		{
			Race race = hairMakeType.Race.Value;
			Tribe tribe = hairMakeType.Tribe.Value;
			Genders gender = (Genders)hairMakeType.Gender;

			foreach (RowRef<CharaMakeCustomize> makeCustomize in hairMakeType.HairStyles)
			{
				if (!makeCustomize.IsValid || makeCustomize.RowId == 0)
					continue;

				this.Add(new CharaMakeCustomizeLibraryEntry(this, race, tribe, gender, makeCustomize, CustomizeIndex.HairStyle));
			}
		}
	}
}

public class FacePaintLibrarySource : CharaMakeCustomizeLibrarySource
{
	public override string? Name => "Face Paint";

	protected override string GetInternalId() => $"FacePaint";

	protected override void Scan()
	{
		ExcelSheet<HairMakeType>? hairMakeTypeSheet = this.Services.GameData.GetSheet<HairMakeType>();

		if (hairMakeTypeSheet == null)
			return;

		foreach (HairMakeType hairMakeType in hairMakeTypeSheet)
		{
			Race race = hairMakeType.Race.Value;
			Tribe tribe = hairMakeType.Tribe.Value;
			Genders gender = (Genders)hairMakeType.Gender;

			foreach (RowRef<CharaMakeCustomize> makeCustomize in hairMakeType.FacePaints)
			{
				if (!makeCustomize.IsValid || makeCustomize.RowId == 0)
					continue;

				this.Add(new CharaMakeCustomizeLibraryEntry(this, race, tribe, gender, makeCustomize, CustomizeIndex.Facepaint));
			}
		}
	}
}
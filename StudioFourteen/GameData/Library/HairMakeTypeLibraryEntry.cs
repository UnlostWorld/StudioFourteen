namespace StudioFourteen.GameData.Library;

using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Library;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System.Threading.Tasks;
using HairMakeType = StudioFourteen.GameData.Sheets.HairMakeType;

public class HairLibraryEntry : LibraryEntryBase
{
	private readonly Race race;
	private readonly Tribe tribe;
	private readonly Genders gender;
	private readonly RowRef<CharaMakeCustomize> makeCustomize;

	public HairLibraryEntry(HairLibrarySource source, Race race, Tribe tribe, Genders gender, RowRef<CharaMakeCustomize> makeCustomize)
		: base(source)
	{
		this.race = race;
		this.tribe = tribe;
		this.gender = gender;
		this.makeCustomize = makeCustomize;

		this.Tags.Add(race.ToTags());
		this.Tags.Add(tribe.ToTags());
		this.Tags.Add(gender.ToTags());
	}

	public override string? SubTitle => $"{this.makeCustomize.Value.FeatureID}";
	public ImageReference? Icon => new ImageReference(this.makeCustomize.Value.Icon);

	public override string? Name
	{
		get
		{
			string? name = this.makeCustomize.Value.HintItem.Value.Name.GetString();
			name = name?.Replace("Modern Aesthetics - ", string.Empty);
			return name;
		}
	}

	public override string ToString() => $"#{this.makeCustomize.RowId}";

	[LibraryMenuTarget(FontAwesome.Sharp.IconChar.PaintBrush, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		if (DalamudServices.ObjectTable == null)
			return;

		unsafe
		{
			Character* pCharacter = (Character*)DalamudServices.ObjectTable.GetObjectAddress(objectTableIndex);
			if (pCharacter == null)
				return;
			pCharacter->SetCustomizeValue(CustomizeIndex.HairStyle, this.makeCustomize.Value.FeatureID, CharacterExtensions.UpdateSource.Library);
		}
	}

	protected override string GetInternalId() => $"{this.makeCustomize.RowId}";
}

public class HairLibrarySource : SourceBase
{
	public override string? Name => "Hair";

	protected override string GetInternalId() => $"Hair";

	protected override void Scan()
	{
		ExcelSheet<HairMakeType>? hairMakeTypeSheet = this.Services.GameData.GetSheet<HairMakeType>();

		if (hairMakeTypeSheet == null)
			return;

		foreach(HairMakeType hairMakeType in hairMakeTypeSheet)
		{
			Race race = hairMakeType.Race.Value;
			Tribe tribe = hairMakeType.Tribe.Value;
			Genders gender = (Genders)hairMakeType.Gender;

			foreach(RowRef<CharaMakeCustomize> makeCustomize in hairMakeType.HairStyles)
			{
				if (!makeCustomize.IsValid || makeCustomize.RowId == 0)
					continue;

				this.Add(new HairLibraryEntry(this, race, tribe, gender, makeCustomize));
			}
		}
	}
}
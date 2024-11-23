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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FontAwesome.Sharp;
using Lumina.Excel.Sheets;
using StudioFourteen.Appearance;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Threading.Tasks;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using BNpcCustomize = StudioFourteen.GameData.Sheets.BNpcCustomize;

public class BNpcBaseLibraryEntry
	: ExcelLibraryEntry, ICharacterAppearance
{
	private readonly BNpcBase bNpcBase;
	private readonly string? name;

	public BNpcBaseLibraryEntry(SourceBase source, BNpcBase npc)
		: base(source, npc.RowId)
	{
		this.bNpcBase = npc;

		CustomizeData? customize = this.Customize;
		if (customize == null)
			return;

		TagCollection? tags = this.bNpcBase.ModelChara.Value.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		tags = customize.Value.GetRace()?.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		tags = customize.Value.GetTribe()?.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		if (GameDataService.BattleNpcNameIndex.TryGetValue($"{this.bNpcBase.RowId}", out int nameRowId))
		{
			BNpcName? name = ServiceManager.Instance.GameData.GetRow<BNpcName>(nameRowId);
			if (name != null)
			{
				this.name = name.Value.Singular.ExtractText();
			}
		}

		if (this.name != null)
		{
			this.Tags.Add("Named");
		}
	}

	public override string? Name => string.IsNullOrEmpty(this.name) ? null : this.name;
	public override string? SubTitle => $"#{this.bNpcBase.RowId}";

	public ImageReference? Icon
	{
		get
		{
			CustomizeData? customize = this.Customize;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public CustomizeData? Customize
	{
		get
		{
			BNpcCustomize? customize = ServiceManager.Instance.GameData.GetRow<BNpcCustomize>(this.bNpcBase.BNpcCustomize.RowId);
			return customize?.Data;
		}
	}

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await Threads.FrameworkThread();

		this.Services.CharacterAppearance.SetModelCharaId(objectTableIndex, this.bNpcBase.ModelChara.Value, UpdateSource.Library);

		if (this.Customize != null)
			this.Services.CharacterAppearance.SetCustomize(objectTableIndex, this.Customize.Value, UpdateSource.Library);

		if (this.bNpcBase.NpcEquip.IsValid)
		{
			foreach(EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
			{
				EquipmentModelId modelId = this.bNpcBase.NpcEquip.Value.GetModelId(slot);
				this.Services.CharacterAppearance.SetEquipment(objectTableIndex, slot, modelId, UpdateSource.Library);
			}
		}
	}
}

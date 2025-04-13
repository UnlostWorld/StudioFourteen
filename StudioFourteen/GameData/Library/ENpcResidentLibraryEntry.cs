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
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Appearance;
using StudioFourteen.DragAndDrop;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.CharacterExtensions;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;
using ENpcBase = StudioFourteen.GameData.Sheets.ENpcBase;

public class ENpcResidentLibraryEntry : ExcelLibraryEntry, ICharacterAppearance
{
	public readonly ENpcResident Npc;

	public ENpcResidentLibraryEntry(SourceBase source, ENpcResident npc)
	: base(source, npc.RowId)
	{
		this.Npc = npc;

		CustomizeData? customize = this.ENpcBase?.CustomizeData;
		if (customize == null)
			return;

		TagCollection? tags = this.ENpcBase?.ModelChara.Value.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		tags = customize.Value.GetRace()?.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		tags = customize.Value.GetTribe()?.ToTags();
		if (tags != null)
			this.Tags.Add(tags);

		if (this.Name != null)
		{
			this.Tags.Add("Named");
		}
	}

	public override string? Name => this.Npc.Singular.GetString();

	public override object? Icon
	{
		get
		{
			CustomizeData? customize = this.ENpcBase?.CustomizeData;
			if (customize == null)
				return null;

			CustomizeData d = customize.Value;
			return d.GetIcon();
		}
	}

	public ENpcBase? ENpcBase => ServiceManager.Instance.GameData.GetRow<ENpcBase>(this.Npc.RowId);

	public IDragSceneInstance CreateSceneInstance() => new CharacterAppearanceDragSceneInstance(this);

	[LibraryMenu(IconChar.Plus, "LOC_AppearanceCreateCharacter")]
	public Task<int> Spawn()
	{
		return ServiceManager.Instance.CharacterLifecycle.CreateAsync(this);
	}

	[LibraryMenuTarget(IconChar.UserShield, "LOC_AppearanceApplyTo")]
	public async Task Apply(int objectTableIndex)
	{
		await TickService.GameTick();

		if (this.ENpcBase == null)
			return;

		this.Services.CharacterAppearance.SetModelCharaId(objectTableIndex, this.ENpcBase.Value.ModelChara.Value, UpdateSource.Library);
		this.Services.CharacterAppearance.SetCustomize(objectTableIndex, this.ENpcBase.Value.CustomizeData, UpdateSource.Library);

		if (this.ENpcBase.Value.NpcEquip.IsValid)
		{
			foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
			{
				WeaponModelId modelId = this.ENpcBase.Value.GetModelId(slot);
				this.Services.CharacterAppearance.SetWeapon(objectTableIndex, slot, modelId, UpdateSource.Library);
			}

			foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
			{
				EquipmentModelId modelId = this.ENpcBase.Value.GetModelId(slot);
				this.Services.CharacterAppearance.SetEquipment(objectTableIndex, slot, modelId, UpdateSource.Library);
			}
		}
	}
}

public class ENpcResidentLibrarySource : ExcelSheetLibrarySource<ENpcResident, ENpcResidentLibraryEntry>
{
	private readonly Dictionary<uint, uint> duplicateRowMap = new();

	public bool IsDuplicate(ENpcResident npcResident)
	{
		return this.duplicateRowMap.ContainsKey(npcResident.RowId);
	}

	protected override void Scan()
	{
		this.Deduplicate();
		base.Scan();
	}

	protected override bool IncludeEntry(ENpcResident row)
	{
		if (this.duplicateRowMap.ContainsKey(row.RowId))
			return false;

		return base.IncludeEntry(row);
	}

	private void Deduplicate()
	{
		this.duplicateRowMap.Clear();

		Stopwatch sw = new();
		sw.Start();
		int count = 0;

		ExcelSheet<ENpcResident>? npcResidentSheet = ServiceManager.Instance.GameData.GetSheet<ENpcResident>();
		if (npcResidentSheet != null)
			this.Deduplicate(npcResidentSheet, ref count);

		sw.Stop();
		this.Log.Information($"Found {count} duplicate appearances in {sw.ElapsedMilliseconds}ms");
	}

	private void Deduplicate(ExcelSheet<ENpcResident> sheet, ref int count)
	{
		Dictionary<string, uint> hashToRowMap = new();
		foreach (ENpcResident npc in sheet)
		{
			string hash = npc.Singular.GetString() + npc.GetAppearanceHash();

			if (hashToRowMap.TryGetValue(hash, out uint originalRowId))
			{
				this.duplicateRowMap.Add(npc.RowId, originalRowId);
				count++;
			}
			else
			{
				hashToRowMap.Add(hash, npc.RowId);
			}
		}
	}
}
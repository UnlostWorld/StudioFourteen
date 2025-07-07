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

namespace StudioFourteen.Scene.GameObjects.Characters;

using System;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Lumina.Excel.Sheets;
using StudioFourteen.Appearance;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;
using StudioFourteen.Services;
using WpfUtils.Commands;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;
using DrawDataContainer = StudioFourteen.Scene.GameObjects.Characters.DrawData.DrawDataContainer;
using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class Character : Skeleton
{
	private ICharacterAppearance? backupAppearance;

	public Character(int objectIndex)
		: base(objectIndex)
	{
		this.RevertAppearanceCommand = new(this.RevertAppearance);
		this.ImportAppearanceCommand = new(this.ImportAppearance);
		this.ExportAppearanceCommand = new(this.ExportAppearance);
	}

	public DrawDataContainer DrawData { get; init; } = new();
	public override object? Icon => Resources.Find("ICON_Type_Character");
	public override string TypeName => Resources.Find("LOC_Type_Character", "Character");

	public SimpleCommand RevertAppearanceCommand { get; init; }
	public SimpleCommand ImportAppearanceCommand { get; init; }
	public SimpleCommand ExportAppearanceCommand { get; init; }

	public unsafe XivCharacter* GetXivCharacter()
	{
		return (XivCharacter*)this.Services.GameObjects.GetXivObject(this.ObjectIndex);
	}

	public unsafe CharaMakeType? GetCharaMakeType()
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.CustomizeData.GetMakeType();
	}

	public override void OnGameTick()
	{
		base.OnGameTick();
		this.DrawData.OnGameTick(this);
	}

	public void BackupAppearance()
	{
		AppearanceFile file = new();
		file.Read(this);
		this.backupAppearance = file;
	}

	public async Task RevertAppearance()
	{
		if (this.backupAppearance == null)
			return;

		await this.backupAppearance.Apply(this, UpdateSource.Restore);
	}

	public void ImportAppearance()
	{
		LibraryPanel.Open(this.Services.Panels.GamePanels);
	}

	public async Task ImportAppearance(ICharacterAppearance appearance, UpdateSource source)
	{
		await appearance.Apply(this, source);
	}

	public async Task SaveAppearance()
	{
		AppearanceFile file = await this.ExportAppearance();
		await this.Services.Files.SaveFileAsync(file, $"{this.Name}'s Appearance");
	}

	public async Task<AppearanceFile> ExportAppearance()
	{
		AppearanceFile file = new();
		await file.ReadAsync(this);
		return file;
	}

	// ----------------------------------------------------------------------
	// Model Chara Id
	// ----------------------------------------------------------------------
	public unsafe uint GetModelCharaId()
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return (uint)pCharacter->ModelContainer.ModelCharaId;
	}

	public unsafe void SetModelCharaId(ModelChara modelChara, UpdateSource source)
	{
		this.SetModelCharaId((int)modelChara.RowId, source);
	}

	public unsafe void SetModelCharaId(int modelCharaId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (pCharacter->ModelContainer.ModelCharaId == modelCharaId)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->ModelContainer.ModelCharaId = modelCharaId;

		this.Services.Redraw.Redraw(this);
		this.Services.CharacterAppearance.RaiseAppearanceChanged(this.ObjectIndex);
	}

	// ----------------------------------------------------------------------
	// Customize Value
	// ----------------------------------------------------------------------
	public unsafe Race? GetRace() => this.Services.GameData.GetRow<Race>(this.GetCustomizeValue(CustomizeIndex.Race));
	public unsafe Tribe? GetTribe() => this.Services.GameData.GetRow<Tribe>(this.GetCustomizeValue(CustomizeIndex.Tribe));

	public unsafe byte GetCustomizeValue(CustomizeIndex index)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.CustomizeData.GetValue(index);
	}

	public unsafe void SetCustomizeValue(CustomizeIndex index, byte value, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		byte oldValue = pCharacter->DrawData.CustomizeData.GetValue(index);
		if (oldValue == value)
			return;

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.CustomizeData.SetValue(index, value);

		if (index == CustomizeIndex.Race
			|| index == CustomizeIndex.Tribe
			|| index == CustomizeIndex.ModelType
			|| index == CustomizeIndex.Gender)
		{
			this.Services.Redraw.Redraw(this);
		}

		this.UpdateCustomize(null, source);
	}

	// ----------------------------------------------------------------------
	// Weapon
	// ----------------------------------------------------------------------
	public unsafe DrawObjectData GetWeapon(WeaponSlot slot)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		if (pCharacter == null)
			return default;

		return pCharacter->DrawData.Weapon(slot);
	}

	public unsafe void SetWeapon(WeaponSlot slot, WeaponModelId modelId, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		// We don't know what it does, lets not mess with it.
		if (slot == WeaponSlot.Unk)
			return;

		// Verify that the weapon is valid, or else the character will just vanish.
		if (modelId.Value != 0)
		{
			ItemLibraryEntry? item = this.Services.GameData.Items?.Find(slot, modelId);
			if (item == null)
			{
				this.Log.Warning($"Attempt to set invalid {slot} model: {modelId.Id}, {modelId.Type}, {modelId.Variant} to character {this}");
				modelId.Value = 0;
			}
		}

		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.LoadWeapon(slot, modelId, 1, 1, 0, 0);
		this.Services.CharacterAppearance.RaiseAppearanceChanged(this.ObjectIndex);
	}

	// ----------------------------------------------------------------------
	// Equipment
	// ----------------------------------------------------------------------
	public unsafe EquipmentModelId GetEquipment(EquipmentSlot slot)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();
		return pCharacter->DrawData.Equipment(slot);
	}

	public unsafe void SetEquipment(Span<EquipmentModelId> equipment, UpdateSource source)
	{
		for (int i = 0; i < equipment.Length; i++)
		{
			EquipmentSlot slot = (EquipmentSlot)i;
			this.SetEquipment(slot, equipment[i], source);
		}
	}

	public unsafe void SetEquipment(EquipmentSlot slot, EquipmentModelId item, UpdateSource source)
	{
		TickService.VerifyGameTickThread();
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore && source != UpdateSource.Preview)
			this.BackupAppearance();

		pCharacter->DrawData.LoadEquipment(slot, &item, true);
		this.Services.CharacterAppearance.RaiseAppearanceChanged(this.ObjectIndex);
	}

	// ----------------------------------------------------------------------
	// Customize
	// ----------------------------------------------------------------------
	public unsafe void SetCustomize(CustomizeData customize, UpdateSource source)
	{
		XivCharacter* pCharacter = this.GetXivCharacter();

		if (pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Race] != customize[(int)CustomizeIndex.Race]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.Tribe] != customize[(int)CustomizeIndex.Tribe]
			|| pCharacter->DrawData.CustomizeData[(int)CustomizeIndex.ModelType] != customize[(int)CustomizeIndex.ModelType])
		{
			this.Services.Redraw.Redraw(this);
		}

		this.UpdateCustomize(customize, source);
	}

	private unsafe void UpdateCustomize(CustomizeData? customize, UpdateSource source)
	{
		TickService.VerifyGameTickThread();

		XivCharacter* pCharacter = this.GetXivCharacter();

		if (source != UpdateSource.Restore)
			this.BackupAppearance();

		CustomizeData* custom = &pCharacter->DrawData.CustomizeData;

		if (customize != null)
			custom->Import(customize.Value);

		bool didLoad = ((Human*)pCharacter->DrawObject)->UpdateDrawData((byte*)custom, true);
		if (!didLoad)
		{
			this.Services.Redraw.Redraw(this);
		}

		this.Services.CharacterAppearance.RaiseAppearanceChanged(this.ObjectIndex);
	}
}
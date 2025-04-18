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

using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;
using StudioFourteen.Appearance;
using StudioFourteen.DragAndDrop;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;

public class MountLibraryEntry : ExcelLibraryEntry, ICharacterAppearance
{
	public readonly Mount Mount;

	public MountLibraryEntry(SourceBase source, Mount mount)
		: base(source, mount.RowId)
	{
		this.Mount = mount;
	}

	public override string? Name => this.Mount.Singular.ToString();
	public override object? Icon => new ImageReference(this.Mount.Icon);
	public override bool IsValid => base.IsValid && this.Mount.ModelChara.RowId != 0;

	public async Task Apply(int objectTableIndex, UpdateSource source)
	{
		await TickService.GameTick();

		this.Services.CharacterAppearance.SetModelCharaId(objectTableIndex, this.Mount.ModelChara.Value, source);
		this.Services.CharacterAppearance.SetWeapon(objectTableIndex, DrawDataContainer.WeaponSlot.MainHand, default, source);
		this.Services.CharacterAppearance.SetWeapon(objectTableIndex, DrawDataContainer.WeaponSlot.OffHand, default, source);

		this.Services.CharacterAppearance.SetEquipment(
			objectTableIndex,
			DrawDataContainer.EquipmentSlot.Head,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Head),
			source);

		this.Services.CharacterAppearance.SetEquipment(
			objectTableIndex,
			DrawDataContainer.EquipmentSlot.Body,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Body),
			source);

		this.Services.CharacterAppearance.SetEquipment(
			objectTableIndex,
			DrawDataContainer.EquipmentSlot.Legs,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Legs),
			source);

		this.Services.CharacterAppearance.SetEquipment(
			objectTableIndex,
			DrawDataContainer.EquipmentSlot.Feet,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Feet),
			source);
	}

	public override IDragSceneInstance? CreateSceneInstance() => new CharacterAppearanceDragSceneInstance(this);

	public EquipmentModelId GetModelId(DrawDataContainer.EquipmentSlot slot)
	{
		EquipmentModelId modelId = default;
		modelId.Value = slot switch
		{
			DrawDataContainer.EquipmentSlot.Head => (ulong)this.Mount.EquipHead,
			DrawDataContainer.EquipmentSlot.Body => (ulong)this.Mount.EquipBody,
			DrawDataContainer.EquipmentSlot.Legs => (ulong)this.Mount.EquipLeg,
			DrawDataContainer.EquipmentSlot.Feet => (ulong)this.Mount.EquipFoot,
			_ => 0UL,
		};

		return modelId;
	}
}
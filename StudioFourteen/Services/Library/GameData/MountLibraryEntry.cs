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

namespace StudioFourteen.Services.Library.GameData.Library;

using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel.Sheets;

public class MountLibraryEntry : ExcelLibraryEntry
{
	public readonly Mount Mount;

	public MountLibraryEntry(SourceBase source, Mount mount)
		: base(source, mount.RowId)
	{
		this.Mount = mount;
	}

	public override string? Name => this.Mount.Singular.ToString();
	public override object? Icon => new TextureIconReference(this.Mount.Icon);
	public override bool IsValid => base.IsValid && this.Mount.ModelChara.RowId != 0;

	/*public async Task Apply(Character character, UpdateSource source)
	{
		await TickService.GameTick();

		character.SetModelCharaId(this.Mount.ModelChara.Value, source);
		character.SetWeapon(DrawDataContainer.WeaponSlot.MainHand, default, source);
		character.SetWeapon(DrawDataContainer.WeaponSlot.OffHand, default, source);

		character.SetEquipment(
			DrawDataContainer.EquipmentSlot.Head,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Head),
			source);

		character.SetEquipment(
			DrawDataContainer.EquipmentSlot.Body,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Body),
			source);

		character.SetEquipment(
			DrawDataContainer.EquipmentSlot.Legs,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Legs),
			source);

		character.SetEquipment(
			DrawDataContainer.EquipmentSlot.Feet,
			this.GetModelId(DrawDataContainer.EquipmentSlot.Feet),
			source);
	}*/

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
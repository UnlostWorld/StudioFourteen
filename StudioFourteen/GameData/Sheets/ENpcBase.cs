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

namespace StudioFourteen.GameData.Sheets;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using System;
using System.Text;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

[Sheet("ENpcBase", 0x5BA9E1A6)]
public readonly unsafe struct ENpcBase(ExcelPage page, uint offset, uint row)
	: IExcelRow<ENpcBase>
{
	public ExcelPage ExcelPage => page;
	public uint RowOffset => offset;
	public uint RowId => row;

	public readonly float Scale => page.ReadFloat32(offset + 144);
	public readonly RowRef<ModelChara> ModelChara => new(page.Module, (uint)page.ReadUInt16(offset + 190), page.Language);
	public readonly RowRef<NpcEquip> NpcEquip => new(page.Module, (uint)page.ReadUInt16(offset + 192), page.Language);
	public readonly bool Visor => page.ReadPackedBool(offset + 256, 1);

	public readonly CustomizeData CustomizeData
	{
		get
		{
			CustomizeData c = default;

			for (int i = 0; i < CustomizeDataExtensions.NumOptions; i++)
			{
				CustomizeIndex index = (CustomizeIndex)i;
				byte val = page.ReadUInt8((nuint)(offset + 202 + i));
				c.SetValue(index, val);
			}

			return c;
		}
	}

	static ENpcBase IExcelRow<ENpcBase>.Create(ExcelPage page, uint offset, uint row) => new(page, offset, row);

	public EquipmentModelId GetModelId(EquipmentSlot slot)
	{
		if (this.NpcEquip.RowId != 0 && this.NpcEquip.IsValid)
			return this.NpcEquip.Value.GetModelId(slot);

		EquipmentModelId modelId = default;
		modelId.Value = page.ReadUInt32(offset + 148 + (4 * (nuint)slot));
		modelId.Stain0 = page.ReadUInt8(offset + 233 + (nuint)slot);
		modelId.Stain1 = page.ReadUInt8(offset + 243 + (nuint)slot);
		return modelId;
	}

	public WeaponModelId GetModelId(WeaponSlot slot)
	{
		if (this.NpcEquip.RowId != 0 && this.NpcEquip.IsValid)
			return this.NpcEquip.Value.GetModelId(slot);

		WeaponModelId modelId = default;
		modelId.Value = page.ReadUInt32(offset + 128 + (8 * (nuint)slot));
		modelId.Stain0 = page.ReadUInt8(offset + 229 + (2 * (nuint)slot));
		modelId.Stain1 = page.ReadUInt8(offset + 230 + (2 * (nuint)slot));
		return modelId;
	}

	public void GetAppearanceHash(ref StringBuilder stringBuilder)
	{
		foreach (WeaponSlot slot in Enum.GetValues<WeaponSlot>())
		{
			WeaponModelId modelId = this.GetModelId(slot);
			stringBuilder.Append(modelId.Id.ToString("X2"));
			stringBuilder.Append(modelId.Stain0.ToString("X2"));
			stringBuilder.Append(modelId.Stain1.ToString("X2"));
		}

		foreach (EquipmentSlot slot in Enum.GetValues<EquipmentSlot>())
		{
			EquipmentModelId modelId = this.GetModelId(slot);
			stringBuilder.Append(modelId.Id.ToString("X2"));
			stringBuilder.Append(modelId.Stain0.ToString("X2"));
			stringBuilder.Append(modelId.Stain1.ToString("X2"));
		}
	}
}
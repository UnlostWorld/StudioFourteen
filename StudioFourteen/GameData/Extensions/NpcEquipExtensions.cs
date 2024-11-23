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

namespace Lumina.Excel.Sheets;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public static class NpcEquipExtensions
{
	public static EquipmentModelId GetModelId(this NpcEquip npcEquip, EquipmentSlot slot)
	{
		EquipmentModelId modelId = default;

		switch (slot)
		{
			case EquipmentSlot.Head:
			{
				modelId.Value = npcEquip.ModelHead;
				modelId.Stain0 = (byte)npcEquip.DyeHead.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Head.RowId;
				break;
			}

			case EquipmentSlot.Body:
			{
				modelId.Value = npcEquip.ModelBody;
				modelId.Stain0 = (byte)npcEquip.DyeBody.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Body.RowId;
				break;
			}

			case EquipmentSlot.Hands:
			{
				modelId.Value = npcEquip.ModelHands;
				modelId.Stain0 = (byte)npcEquip.DyeHands.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Hands.RowId;
				break;
			}

			case EquipmentSlot.Legs:
			{
				modelId.Value = npcEquip.ModelLegs;
				modelId.Stain0 = (byte)npcEquip.DyeLegs.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Legs.RowId;
				break;
			}

			case EquipmentSlot.Feet:
			{
				modelId.Value = npcEquip.ModelFeet;
				modelId.Stain0 = (byte)npcEquip.DyeFeet.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Feet.RowId;
				break;
			}

			case EquipmentSlot.Ears:
			{
				modelId.Value = npcEquip.ModelEars;
				modelId.Stain0 = (byte)npcEquip.DyeEars.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Ears.RowId;
				break;
			}

			case EquipmentSlot.Neck:
			{
				modelId.Value = npcEquip.ModelNeck;
				modelId.Stain0 = (byte)npcEquip.DyeNeck.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Neck.RowId;
				break;
			}

			case EquipmentSlot.Wrists:
			{
				modelId.Value = npcEquip.ModelWrists;
				modelId.Stain0 = (byte)npcEquip.DyeWrists.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2Wrists.RowId;
				break;
			}

			case EquipmentSlot.RFinger:
			{
				modelId.Value = npcEquip.ModelRightRing;
				modelId.Stain0 = (byte)npcEquip.DyeRightRing.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2RightRing.RowId;
				break;
			}

			case EquipmentSlot.LFinger:
			{
				modelId.Value = npcEquip.ModelLeftRing;
				modelId.Stain0 = (byte)npcEquip.DyeLeftRing.RowId;
				modelId.Stain1 = (byte)npcEquip.Dye2LeftRing.RowId;
				break;
			}
		}

		return modelId;
	}
}

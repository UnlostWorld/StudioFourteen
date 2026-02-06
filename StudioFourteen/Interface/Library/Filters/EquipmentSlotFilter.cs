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

namespace StudioFourteen.Interface.Library.Filters;

using System;
using PropertyGenerator.Avalonia;
using StudioFourteen.Services.Library;
using StudioFourteen.Services.Library.GameData.Library;
using StudioFourteen.Services.Library.GameData.Extensions;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public partial class EquipmentSlotFilter : FilterBase
{
	private EquipmentSlot slot;

	[GeneratedStyledProperty]
	public partial EquipmentSlot Slot { get; set; }

	public override void Freeze()
	{
		this.slot = this.Slot;
	}

	public override bool Filter(LibraryEntryBase entry)
	{
		if (entry is ItemLibraryEntry itemEntry)
		{
			if (itemEntry.EquipSlot?.Contains(this.slot) == true)
			{
				return true;
			}
		}

		return false;
	}
}
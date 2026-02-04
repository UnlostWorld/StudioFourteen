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

namespace StudioFourteen.Services.Library.Filters;

using System;
using StudioFourteen.Services.Library.GameData.Extensions;
using StudioFourteen.Services.Library.GameData.Library;

using static FFXIVClientStructs.FFXIV.Client.Game.Character.DrawDataContainer;

public partial class EquipmentFilter : FilterBase
{
	public EquipmentFilter(string param)
		: base(param)
	{
		this.Slot = Enum.Parse<EquipmentSlot>(param);
	}

	public EquipmentSlot Slot { get; set; }

	public override bool Filter(LibraryEntryBase entry)
	{
		if (entry is ItemLibraryEntry itemEntry)
		{
			if (itemEntry.EquipSlot?.Contains(this.Slot) == true)
			{
				return true;
			}
		}

		return false;
	}
}
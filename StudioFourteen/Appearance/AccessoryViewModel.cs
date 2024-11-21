// .                    @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Appearance;

using StudioFourteen.GameData.Library;

public enum AccessorySlots
{
	Glasses,
}

public class AccessoryViewModel : ExcelRowItemViewModel<GlassesLibraryEntry>
{
	public AccessoryViewModel(AccessorySlots slot)
	{
		this.Slot = slot;
	}

	public AccessorySlots Slot { get; init; }

	protected unsafe override ushort LiveValue
	{
		get => this.Target->DrawData.GlassesIds[(int)this.Slot];
		set => this.Target->DrawData.SetGlasses((int)this.Slot, value);
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Glasses", "Glasses");
}

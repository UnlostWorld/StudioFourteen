// .                      @@             _____ _______ _    _ _____ _____ ____
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

namespace StudioFourteen.Input;

public enum KeyBindEvents
{
	Save,
	SaveAs,
	InvokeQuickSearch,

	FreeCamera_MoveLeft,
	FreeCamera_MoveRight,
	FreeCamera_MoveUp,
	FreeCamera_MoveDown,
	FreeCamera_MoveForwards,
	FreeCamera_MoveBack,

	FreeCamera_YawLeft,
	FreeCamera_YawRight,
	FreeCamera_PitchUp,
	FreeCamera_PitchDown,
	FreeCamera_RollLeft,
	FreeCamera_RollRight,

	OrbitCamera_PanLeft,
	OrbitCamera_PanRight,
	OrbitCamera_PanUp,
	OrbitCamera_PanDown,
	OrbitCamera_RollLeft,
	OrbitCamera_RollRight,
	OrbitCamera_MoveUp,
	OrbitCamera_MoveDown,
	OrbitCamera_MoveLeft,
	OrbitCamera_MoveRight,
}
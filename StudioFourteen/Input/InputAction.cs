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

namespace StudioFourteen.Input;

// NOTE: these are in priority order, with elements at the top
// taking precedence over elements at the bottom.
public enum InputAction
{
	Focus_Game,

	Handle_Select,
	Handle_Up,
	Handle_Down,
	Handle_Left,
	Handle_Right,

	Navigate_Up,
	Navigate_Down,
	Navigate_Left,
	Navigate_Right,
	Navigate_Enter,
	Navigate_Back,
	Navigate_TabRight,
	Navigate_TabLeft,

	Save,
	SaveAs,
	InvokeQuickSearch,

	NextTarget,
	PreviousTarget,

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
	FreeCamera_RotateLeft,
	FreeCamera_RotateRight,
	FreeCamera_RotateUp,
	FreeCamera_RotateDown,

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
	OrbitCamera_MoveForward,
	OrbitCamera_MoveBackward,
	OrbitCamera_ZoomIn,
	OrbitCamera_ZoomOut,
	OrbitCamera_RotateLeft,
	OrbitCamera_RotateRight,
	OrbitCamera_RotateUp,
	OrbitCamera_RotateDown,
}
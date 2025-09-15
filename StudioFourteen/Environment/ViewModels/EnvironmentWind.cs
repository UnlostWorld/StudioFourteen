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

namespace StudioFourteen.Environment;

using StudioFourteen.Interop.Structs.Environment;

public partial class EnvironmentWind : EnvironmentComponentBase
{
	[Bind] public partial float Direction { get; set; }
	[Bind] public partial float Angle { get; set; }
	[Bind] public partial float Speed { get; set; }

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.Direction = pModel->Wind.Direction;
		this.Angle = pModel->Wind.Angle;
		this.Speed = pModel->Wind.Speed;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Wind.Direction = this.Direction;
		pModel->Wind.Angle = this.Angle;
		pModel->Wind.Speed = this.Speed;
	}
}
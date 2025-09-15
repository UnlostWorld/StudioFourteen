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

using System.Numerics;
using StudioFourteen.Interop.Structs.Environment;

public partial class EnvironmentRain : EnvironmentComponentBase
{
	[Bind] public partial float Raindrops { get; set; }
	[Bind] public partial float Intensity { get; set; }
	[Bind] public partial float Weight { get; set; }
	[Bind] public partial float Scatter { get; set; }
	[Bind] public partial float Size { get; set; }
	[Bind] public partial Vector4 Color { get; set; }

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.Raindrops = pModel->Rain.Raindrops;
		this.Intensity = pModel->Rain.Intensity;
		this.Weight = pModel->Rain.Weight;
		this.Scatter = pModel->Rain.Scatter;
		this.Size = pModel->Rain.Size;
		this.Color = pModel->Rain.Color;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Rain.Raindrops = this.Raindrops;
		pModel->Rain.Intensity = this.Intensity;
		pModel->Rain.Weight = this.Weight;
		pModel->Rain.Scatter = this.Scatter;
		pModel->Rain.Size = this.Size;
		pModel->Rain.Color = this.Color;
	}
}

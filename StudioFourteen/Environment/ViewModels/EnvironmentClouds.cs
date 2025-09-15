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

public partial class EnvironmentClouds : EnvironmentComponentBase
{
	[Bind] public partial Vector3 CloudColor { get; set; }
	[Bind] public partial Vector3 Color2 { get; set; }
	[Bind] public partial float Gradient { get; set; }
	[Bind] public partial float SideHeight { get; set; }
	[Bind] public partial uint CloudTexture { get; set; }
	[Bind] public partial uint CloudSideTexture { get; set; }

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.CloudColor = pModel->Clouds.CloudColor;
		this.Color2 = pModel->Clouds.Color2;
		this.Gradient = pModel->Clouds.Gradient;
		this.SideHeight = pModel->Clouds.SideHeight;
		this.CloudTexture = pModel->Clouds.CloudTexture;
		this.CloudSideTexture = pModel->Clouds.CloudSideTexture;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Clouds.CloudColor = this.CloudColor;
		pModel->Clouds.Color2 = this.Color2;
		pModel->Clouds.Gradient = this.Gradient;
		pModel->Clouds.SideHeight = this.SideHeight;
		pModel->Clouds.CloudTexture = this.CloudTexture;
		pModel->Clouds.CloudSideTexture = this.CloudSideTexture;
	}
}

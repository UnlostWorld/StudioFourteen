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
using PropertyChanged.SourceGenerator;
using StudioFourteen.Interop.Structs.Environment;

public partial class EnvironmentClouds : EnvironmentComponentBase
{
	[Notify] private Vector3 cloudColor;
	[Notify] private Vector3 color2;
	[Notify] private float gradient;
	[Notify] private float sideHeight;
	[Notify] private uint cloudTexture;
	[Notify] private uint cloudSideTexture;

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

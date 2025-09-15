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

public partial class EnvironmentLighting : EnvironmentComponentBase
{
	[Bind] public partial Vector3 SunLightColor { get; set; }
	[Bind] public partial Vector3 MoonLightColor { get; set; }
	[Bind] public partial Vector3 Ambient { get; set; }
	[Bind] public partial float AmbientSaturation { get; set; }
	[Bind] public partial float Temperature { get; set; }

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.SunLightColor = pModel->Lighting.SunLightColor;
		this.MoonLightColor = pModel->Lighting.MoonLightColor;
		this.Ambient = pModel->Lighting.Ambient;
		this.AmbientSaturation = pModel->Lighting.AmbientSaturation;
		this.Temperature = pModel->Lighting.Temperature;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Lighting.SunLightColor = this.SunLightColor;
		pModel->Lighting.MoonLightColor = this.MoonLightColor;
		pModel->Lighting.Ambient = this.Ambient;
		pModel->Lighting.AmbientSaturation = this.AmbientSaturation;
		pModel->Lighting.Temperature = this.Temperature;
	}
}

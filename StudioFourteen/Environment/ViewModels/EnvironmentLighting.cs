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

public partial class EnvironmentLighting : EnvironmentComponentBase
{
	[Notify] private Vector3 sunLightColor;
	[Notify] private Vector3 moonLightColor;
	[Notify] private Vector3 ambient;
	[Notify] private float ambientSaturation;
	[Notify] private float temperature;

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

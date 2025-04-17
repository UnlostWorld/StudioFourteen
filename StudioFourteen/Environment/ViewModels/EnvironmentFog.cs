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

public partial class EnvironmentFog : EnvironmentComponentBase
{
	[Notify] private Vector4 color;
	[Notify] private float distance;
	[Notify] private float thickness;
	[Notify] private float opacity;
	[Notify] private float skyVisibility;

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.Color = pModel->Fog.Color;
		this.Distance = pModel->Fog.Distance;
		this.Thickness = pModel->Fog.Thickness;
		this.Opacity = pModel->Fog.Opacity;
		this.SkyVisibility = pModel->Fog.SkyVisibility;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Fog.Color = this.Color;
		pModel->Fog.Distance = this.Distance;
		pModel->Fog.Thickness = this.Thickness;
		pModel->Fog.Opacity = this.Opacity;
		pModel->Fog.SkyVisibility = this.SkyVisibility;
	}
}

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

public partial class EnvironmentStars : EnvironmentComponentBase
{
	[Notify] private float constellationIntensity;
	[Notify] private float constellations;
	[Notify] private float stars;
	[Notify] private float galaxyIntensity;
	[Notify] private float starIntensity;
	[Notify] private Vector4 moonColor;
	[Notify] private float moonBrightness;

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.ConstellationIntensity = pModel->Stars.ConstellationIntensity;
		this.Constellations = pModel->Stars.Constellations;
		this.Stars = pModel->Stars.Stars;
		this.GalaxyIntensity = pModel->Stars.GalaxyIntensity;
		this.StarIntensity = pModel->Stars.StarIntensity;
		this.MoonColor = pModel->Stars.MoonColor;
		this.MoonBrightness = pModel->Stars.MoonBrightness;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Stars.ConstellationIntensity = this.ConstellationIntensity;
		pModel->Stars.Constellations = this.Constellations;
		pModel->Stars.Stars = this.Stars;
		pModel->Stars.GalaxyIntensity = this.GalaxyIntensity;
		pModel->Stars.StarIntensity = this.StarIntensity;
		pModel->Stars.MoonColor = this.MoonColor;
		pModel->Stars.MoonBrightness = this.MoonBrightness;
	}
}

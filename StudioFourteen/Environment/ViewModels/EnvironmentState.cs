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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Interop.Structs.Environment;
using StudioFourteen.Services;

public partial class EnvironmentState
{
	[Notify] private bool freezeSkyTexture = false;
	[Notify] private SkyTextureLibraryEntry? skyTexture;

	public EnvironmentLighting Lighting { get; init; } = new();
	public EnvironmentStars Stars { get; init; } = new();
	public EnvironmentFog Fog { get; init; } = new();
	public EnvironmentClouds Clouds { get; init; } = new();
	public EnvironmentRain Rain { get; init; } = new();
	public EnvironmentDust Dust { get; init; } = new();
	public EnvironmentWind Wind { get; init; } = new();

	public unsafe void ReadFrom(EnvState* pModel)
	{
		TickService.VerifyGameTickThread();

		if (!this.FreezeSkyTexture && (this.SkyTexture == null || this.SkyTexture.SkyTexId != pModel->SkyId))
			this.SkyTexture = ServiceManager.Instance.Environment.SkyTextureSource.Get(pModel->SkyId);

		this.Lighting.CheckAndReadFrom(pModel);
		this.Stars.CheckAndReadFrom(pModel);
		this.Fog.CheckAndReadFrom(pModel);
		this.Clouds.CheckAndReadFrom(pModel);
		this.Rain.CheckAndReadFrom(pModel);
		this.Dust.CheckAndReadFrom(pModel);
		this.Wind.CheckAndReadFrom(pModel);
	}

	public unsafe void WriteTo(EnvState* pModel)
	{
		TickService.VerifyGameTickThread();

		if (this.FreezeSkyTexture && this.SkyTexture != null)
			pModel->SkyId = this.SkyTexture.SkyTexId;

		this.Lighting.CheckAndWriteTo(pModel);
		this.Stars.CheckAndWriteTo(pModel);
		this.Fog.CheckAndWriteTo(pModel);
		this.Clouds.CheckAndWriteTo(pModel);
		this.Rain.CheckAndWriteTo(pModel);
		this.Dust.CheckAndWriteTo(pModel);
		this.Wind.CheckAndWriteTo(pModel);
	}
}

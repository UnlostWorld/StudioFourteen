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

public partial class EnvironmentDust : EnvironmentComponentBase
{
	[Notify] private float intensity;
	[Notify] private float weight;
	[Notify] private float spread;
	[Notify] private float speed;
	[Notify] private float size;
	[Notify] private Vector4 color;
	[Notify] private float glow;
	[Notify] private float spin;
	[Notify] private uint textureId;

	public unsafe override void ReadFrom(EnvState* pModel)
	{
		this.Intensity = pModel->Dust.Intensity;
		this.Weight = pModel->Dust.Weight;
		this.Spread = pModel->Dust.Spread;
		this.Speed = pModel->Dust.Speed;
		this.Size = pModel->Dust.Size;
		this.Color = pModel->Dust.Color;
		this.Glow = pModel->Dust.Glow;
		this.Spin = pModel->Dust.Spin;
		this.TextureId = pModel->Dust.TextureId;
	}

	public unsafe override void WriteTo(EnvState* pModel)
	{
		pModel->Dust.Intensity = this.Intensity;
		pModel->Dust.Weight = this.Weight;
		pModel->Dust.Spread = this.Spread;
		pModel->Dust.Speed = this.Speed;
		pModel->Dust.Size = this.Size;
		pModel->Dust.Color = this.Color;
		pModel->Dust.Glow = this.Glow;
		pModel->Dust.Spin = this.Spin;
		pModel->Dust.TextureId = this.TextureId;
	}
}

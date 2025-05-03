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

namespace StudioFourteen.Rendering.Materials;

using System.Runtime.InteropServices;

public class GridMaterial : InstanceMaterialBase<GridMaterial.GridInstanceData>
{
	protected override ShaderLoader VertexShader =>
		new EmbeddedShaderLoader("Grid.hlsl", "vs_4_0", "vert");

	protected override ShaderLoader PixelShader =>
		new EmbeddedShaderLoader("Grid.hlsl", "ps_4_0", "pixel");

	protected override void SetDefault(ref GridInstanceData instance)
	{
		base.SetDefault(ref instance);

		instance.Color = Color.White;
		instance.GridSize = 1.0f;
		instance.LineThickness = 0.1f;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GridInstanceData
	{
		public Color Color;
		public float GridSize;
		public float LineThickness;
		public float Height;
		public float Unused3;
	}
}

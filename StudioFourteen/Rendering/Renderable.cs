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

namespace StudioFourteen.Rendering;

using System;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Geometry;
using StudioFourteen.Rendering.Materials;

public class Renderable : IDisposable
{
	public MaterialBase? Material;
	public GeometryBase? Geometry;

	public Renderable()
	{
	}

	public Renderable(MaterialBase material, GeometryBase geometry)
	{
		this.Material = material;
		this.Geometry = geometry;
	}

	public void Draw(Device device, DeviceContext deviceContext)
	{
		if (this.Material == null)
			return;

		if (this.Geometry == null)
			return;

		if (!this.Material.IsLoaded)
			this.Material.Load(device);

		if (!this.Geometry.IsLoaded)
			this.Geometry.Load(device);

		this.Material.Bind(deviceContext);
		this.Geometry.Bind(deviceContext);
		this.Geometry.Draw(deviceContext);
	}

	public void Dispose()
	{
		this.Material?.Dispose();
		this.Geometry?.Dispose();
	}
}
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
	public Material? Material;
	public GeometryBase? Geometry;

	private Exception? materialException;
	private Exception? geometryException;

	public Renderable()
	{
	}

	public Renderable(Material material, GeometryBase geometry)
	{
		this.Material = material;
		this.Geometry = geometry;
	}

	public void Draw(RenderingService service, Device device, DeviceContext deviceContext)
	{
		if (this.Material == null || this.materialException != null)
			return;

		if (this.Geometry == null || this.geometryException != null)
			return;

		if (!this.Material.IsLoaded)
		{
			try
			{
				this.Material.Load(device);
			}
			catch (Exception ex)
			{
				this.materialException = ex;
				service.LogInternalError($"Error loading material: {this.Material}", ex);
				return;
			}
		}

		if (!this.Geometry.IsLoaded)
		{
			try
			{
				this.Geometry.Load(device);
			}
			catch (Exception ex)
			{
				this.geometryException = ex;
				service.LogInternalError($"Error loading geometry: {this.Geometry}", ex);
				return;
			}
		}

		this.Material.Bind(deviceContext);
		this.Geometry.Bind(deviceContext);
		this.Geometry.Draw(deviceContext);
	}

	public virtual void Dispose()
	{
		this.Material?.Dispose();
		this.Geometry?.Dispose();
	}
}
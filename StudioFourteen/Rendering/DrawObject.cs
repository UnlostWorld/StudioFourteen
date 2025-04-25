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

public class DrawObject : DrawBase
{
	public Color Color = Color.White;

	public Material? Material;
	public Geometry? Geometry;

	private Exception? materialException;
	private Exception? geometryException;

	public DrawObject()
	{
	}

	public DrawObject(Material material, Geometry geometry)
	{
		this.Material = material;
		this.Geometry = geometry;
	}

	public override void Draw(Transform transform, DrawState drawState)
	{
		if (this.Material == null || this.materialException != null)
			return;

		if (this.Geometry == null || this.geometryException != null)
			return;

		if (drawState.Device == null)
			return;

		if (!this.Material.IsLoaded)
		{
			try
			{
				this.Material.Load(drawState.Device);
			}
			catch (Exception ex)
			{
				this.materialException = ex;
				Logging.Shared.Error(ex, $"Error loading material: {this.Material}");
				return;
			}
		}

		if (!this.Geometry.IsLoaded)
		{
			try
			{
				this.Geometry.Load(drawState.Device);
			}
			catch (Exception ex)
			{
				this.geometryException = ex;
				Logging.Shared.Error(ex, $"Error loading geometry: {this.Geometry}");
				return;
			}
		}

		Transform thisTransform = transform * this.Transform;
		drawState.Draw(this.Color, thisTransform, this.Material, this.Geometry);
	}

	public override void Dispose()
	{
		this.Material?.Dispose();
		this.Geometry?.Dispose();
	}
}
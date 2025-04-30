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
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Geometries;
using StudioFourteen.Rendering.Materials;
using Device = SharpDX.Direct3D11.Device;

public class DrawObject : DrawBase
{
	public Color Color = Color.White;

	public MaterialBase? Material;
	public GeometryBase? Geometry;

	private Exception? materialException;
	private Exception? geometryException;

	public DrawObject()
	{
	}

	public DrawObject(MaterialBase material, GeometryBase geometry)
	{
		this.Material = material;
		this.Geometry = geometry;
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
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
				Logging.Shared.Error(ex, $"Error loading material: {this.Material}");
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
				Logging.Shared.Error(ex, $"Error loading geometry: {this.Geometry}");
				return;
			}
		}

		Transform thisTransform = transform * this.Transform;

		this.Material.Bind(this, device, deviceContext);
		this.Geometry.Bind(this, device, deviceContext);

		////this.Data.ObjectTransform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		////this.Data.ObjectColor = this.Color;
		////this.DeviceContext.UpdateSubresource(ref this.Data, this.constantsBuffer);

		this.Geometry.Draw(deviceContext);
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, ref HitTestResult result)
	{
		Transform thisTransform = transform * this.Transform;
		this.Geometry?.HitTest(screenPosition, thisTransform, viewProjection, ref result);

		if (result.Geometry == this.Geometry)
		{
			result.DrawObject = this;
		}
	}

	public override void Dispose()
	{
		this.Material?.Dispose();
		this.Geometry?.Dispose();
	}
}
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
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

public class DrawState(int slot)
{
	public DrawData Data;

	private Buffer? constantsBuffer;

	public Device? Device { get; private set; }
	public DeviceContext? DeviceContext { get; private set; }

	public void Bind(Device device, DeviceContext context)
	{
		this.Device = device;
		this.DeviceContext = context;

		if (this.constantsBuffer == null)
		{
			this.constantsBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<DrawData>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		context.VertexShader.SetConstantBuffer(slot, this.constantsBuffer);
		context.GeometryShader.SetConstantBuffer(slot, this.constantsBuffer);
		context.PixelShader.SetConstantBuffer(slot, this.constantsBuffer);
	}

	public void Dispose()
	{
		this.constantsBuffer?.Dispose();
		this.constantsBuffer = null;
	}

	public void Draw(Color color, Transform transform, Material material, Geometry geometry)
	{
		if (this.DeviceContext == null)
			return;

		material.Bind(this.DeviceContext);
		geometry.Bind(this.DeviceContext);

		this.Data.ObjectTransform = Matrix4x4.Transpose(transform.ToMatrix());
		this.Data.ObjectColor = color;
		this.DeviceContext.UpdateSubresource(ref this.Data, this.constantsBuffer);

		geometry.Draw(this.DeviceContext);
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct DrawData
	{
		public Vector4 ClippingPlanes;
		public Matrix4x4 ViewProjection;
		public Matrix4x4 ObjectTransform;
		public Color ObjectColor;
	}
}
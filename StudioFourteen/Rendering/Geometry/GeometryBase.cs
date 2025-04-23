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

namespace StudioFourteen.Rendering.Geometry;

using System;
using System.Numerics;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

public abstract class GeometryBase : IDisposable
{
	private Buffer? vertices;
	private Buffer? indices;
	private int indexLength = 0;

	public bool IsLoaded => this.vertices != null && this.indices != null;

	public void Load(Device device)
	{
		Vertex[] vertices;
		ushort [] indices;
		this.Load(out vertices, out indices);

		this.indexLength = indices.Length;
		this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
		this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
	}

	public void Bind(DeviceContext context)
	{
		context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0));
		context.InputAssembler.SetIndexBuffer(this.indices, SharpDX.DXGI.Format.R16_UInt, 0);
	}

	public void Draw(DeviceContext context)
	{
		context.DrawIndexed(this.indexLength, 0, 0);
	}

	public void Dispose()
	{
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}

	protected abstract void Load(out Vertex[] vertices, out ushort[] indices);
}

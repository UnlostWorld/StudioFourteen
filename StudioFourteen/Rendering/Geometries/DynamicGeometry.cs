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

namespace StudioFourteen.Rendering.Geometries;

using System;
using System.Numerics;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

using Buffer = SharpDX.Direct3D11.Buffer;

public abstract class DynamicGeometryBase : GeometryBase
{
	protected Vertex[] vertices;
	protected ushort[]? indices;

	private Buffer? vertexBuffer;
	private VertexBufferBinding vertexBufferBinding;
	private Buffer? indexBuffer;

	public DynamicGeometryBase(int vertexCount, int indexCount = 0)
	{
		this.vertices = new Vertex[vertexCount];
		this.indices = new ushort[indexCount];
	}

	public override bool IsLoaded => this.vertexBuffer != null;
	protected virtual PrimitiveTopology TopologyType => PrimitiveTopology.LineList;

	public override void Load(Device device)
	{
		BufferDescription desc = new BufferDescription(
			SharpDX.Utilities.SizeOf<Vertex>() * this.vertices.Length,
			ResourceUsage.Dynamic,
			BindFlags.VertexBuffer,
			CpuAccessFlags.Write,
			ResourceOptionFlags.None,
			0);

		this.vertexBuffer = Buffer.Create(device, this.vertices, desc);
		this.vertexBufferBinding = new VertexBufferBinding(this.vertexBuffer, SharpDX.Utilities.SizeOf<Vertex>(), 0);

		if (this.indices != null)
		{
			BufferDescription iDesc = new BufferDescription(
				SharpDX.Utilities.SizeOf<ushort>() * this.indices.Length,
				ResourceUsage.Dynamic,
				BindFlags.IndexBuffer,
				CpuAccessFlags.Write,
				ResourceOptionFlags.None,
				0);

			this.indexBuffer = Buffer.Create(device, this.indices, iDesc);
		}
	}

	public override void Bind(DeviceContext context)
	{
		DataStream dataStream;
		context.MapSubresource(this.vertexBuffer, MapMode.WriteDiscard, MapFlags.None, out dataStream);
		dataStream.WriteRange(this.vertices);
		context.UnmapSubresource(this.vertexBuffer, 0);

		if (this.indices != null)
		{
			context.MapSubresource(this.indexBuffer, MapMode.WriteDiscard, MapFlags.None, out dataStream);
			dataStream.WriteRange(this.indices);
			context.UnmapSubresource(this.indexBuffer, 0);
		}

		context.InputAssembler.PrimitiveTopology = this.TopologyType;
		context.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
		context.InputAssembler.SetIndexBuffer(this.indexBuffer, SharpDX.DXGI.Format.R16_UInt, 0);
	}

	public override void Draw(DeviceContext context)
	{
		if (this.indices == null)
		{
			context.Draw(this.vertices.Length, 0);
		}
		else
		{
			context.DrawIndexed(this.indices.Length, 0, 0);
		}
	}

	public override void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		ref HitTestResult result)
	{
		// Not Supported. =(
	}

	public override void Dispose()
	{
		this.vertexBuffer?.Dispose();
		this.indexBuffer?.Dispose();
	}
}

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

using System.Numerics;
using SharpDX.Direct3D11;

public abstract class MeshGeometryBase : GeometryBase
{
	private Mesh? mesh;

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;
	private Buffer? indices;
	private int indexLength = 0;

	public override bool IsLoaded => this.vertices != null;

	public override void Load(Device device)
	{
		this.mesh = this.LoadMesh();

		Vertex[] vertices = this.mesh.Vertices.ToArray();
		this.vertexLength = vertices.Length;
		this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
		this.vertexBufferBinding = new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0);

		if (this.mesh.Indices != null)
		{
			ushort[] indices = this.mesh.Indices.ToArray();
			this.indexLength = indices.Length;
			this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
		}
	}

	public override void Bind(DeviceContext context)
	{
		if (this.mesh == null)
			return;

		context.InputAssembler.PrimitiveTopology = this.mesh.Topology;
		context.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
		context.InputAssembler.SetIndexBuffer(this.indices, SharpDX.DXGI.Format.R16_UInt, 0);
	}

	public override void Draw(DeviceContext context)
	{
		if (this.indices == null)
		{
			context.Draw(this.vertexLength, 0);
		}
		else
		{
			context.DrawIndexed(this.indexLength, 0, 0);
		}
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, ref HitTestResult result)
	{
		if (this.mesh == null)
			this.mesh = this.LoadMesh();

		this.mesh.HitTest(screenPosition, transform, viewProjection, ref result);
	}

	public override void Dispose()
	{
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}

	protected abstract Mesh LoadMesh();
}
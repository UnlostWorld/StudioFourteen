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
using StudioFourteen.Rendering.Meshes;

using Buffer = SharpDX.Direct3D11.Buffer;

public abstract class Geometry() : IDisposable
{
	public static readonly EmbeddedGeometry Cube = new("Cube.jsonc");
	public static readonly EmbeddedGeometry FlatCube = new("FlatCube.jsonc");
	public static readonly EmbeddedGeometry Quad = new("Quad.jsonc");
	public static readonly EmbeddedGeometry WireCube = new("WireCube.jsonc");
	public static readonly GeneratedGeometry<WireCircle> WireCircle = new();

	private Buffer? vertices;
	private int vertexLength = 0;
	private Buffer? indices;
	private int indexLength = 0;

	private Mesh? mesh;

	public bool IsLoaded => this.vertices != null;

	public void Load(Device device)
	{
		this.mesh = this.GetMesh();

		Vertex[] vertices = this.mesh.Vertices.ToArray();
		this.vertexLength = vertices.Length;
		this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices);

		if (this.mesh.Indices != null)
		{
			ushort [] indices = this.mesh.Indices.ToArray();
			this.indexLength = indices.Length;
			this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
		}
	}

	public void Bind(DeviceContext context)
	{
		if (this.mesh == null)
			return;

		context.InputAssembler.PrimitiveTopology = this.mesh.Topology;
		context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0));
		context.InputAssembler.SetIndexBuffer(this.indices, SharpDX.DXGI.Format.R16_UInt, 0);
	}

	public void Draw(DeviceContext context)
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

	public void Dispose()
	{
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}

	public Mesh GetMesh()
	{
		if (this.mesh == null)
			this.mesh = this.LoadMesh();

		return this.mesh;
	}

	protected abstract Mesh LoadMesh();
}

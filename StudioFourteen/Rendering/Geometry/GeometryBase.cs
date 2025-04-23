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
	private Buffer? buffer;
	private int vertexLength = 0;

	public bool IsLoaded => this.buffer != null;

	public abstract Vertex[] Vertices { get; }

	public void Load(Device device)
	{
		Vertex[] vertices = this.Vertices;
		this.vertexLength = vertices.Length;
		this.buffer = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
	}

	public void Bind(DeviceContext context)
	{
		context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
		context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(this.buffer, SharpDX.Utilities.SizeOf<Vertex>(), 0));
	}

	public void Draw(DeviceContext context)
	{
		context.Draw(this.vertexLength, 0);
	}

	public void Dispose()
	{
		this.buffer?.Dispose();
	}
}

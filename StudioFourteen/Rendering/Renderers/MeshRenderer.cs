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
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Materials;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

public class MeshRenderer : RendererBase
{
	public MaterialBase? Material;
	public Mesh? Mesh;

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;
	private Buffer? indices;
	private int indexLength = 0;

	private Exception? materialException;

	public MeshRenderer()
	{
	}

	public MeshRenderer(Mesh mesh, MaterialBase material)
	{
		this.Mesh = mesh;
		this.Material = material;
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (this.Material == null || this.materialException != null)
			return;

		if (this.Mesh == null)
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

		if (this.vertices == null)
		{
			Vertex[] vertices = this.Mesh.Vertices.ToArray();
			this.vertexLength = vertices.Length;
			this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
			this.vertexBufferBinding = new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0);

			if (this.Mesh.Indices != null)
			{
				ushort[] indices = this.Mesh.Indices.ToArray();
				this.indexLength = indices.Length;
				this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
			}
		}

		Transform thisTransform = transform * this.Transform;

		this.Material.Bind(this, device, deviceContext);

		deviceContext.InputAssembler.PrimitiveTopology = this.Mesh.Topology;
		deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
		deviceContext.InputAssembler.SetIndexBuffer(this.indices, SharpDX.DXGI.Format.R16_UInt, 0);

		////this.Data.ObjectTransform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		////this.DeviceContext.UpdateSubresource(ref this.Data, this.constantsBuffer);

		if (this.indices == null)
		{
			deviceContext.Draw(this.vertexLength, 0);
		}
		else
		{
			deviceContext.DrawIndexed(this.indexLength, 0, 0);
		}
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, ref HitTestResult result)
	{
		Transform thisTransform = transform * this.Transform;
		this.Mesh?.HitTest(screenPosition, thisTransform, viewProjection, ref result);

		if (result.Mesh == this.Mesh)
		{
			result.Renderer = this;
		}
	}

	public override void Dispose()
	{
		this.Material?.Dispose();
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}
}
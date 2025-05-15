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

namespace StudioFourteen.Rendering.Scene;

using System;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using StudioFourteen.Content;
using StudioFourteen.Rendering.Materials;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

public class MeshRenderer : InstanceRendererBase<MeshRenderer.PerRendererData>
{
	public MaterialBase? Material;
	public IContent<Mesh>? Mesh;

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;
	private Buffer? indices;
	private int indexLength = 0;

	private Exception? materialException;

	public MeshRenderer()
	{
		this.Color = Color.White;
	}

	public MeshRenderer(IContent<Mesh> mesh, MaterialBase material)
	{
		this.Mesh = mesh;
		this.Material = material;
		this.Color = Color.White;
	}

	public Color Color
	{
		get => this.Data.Color;
		set => this.Data.Color = value;
	}

	public ref TDataType GetMaterialInstance<TDataType>()
		where TDataType : unmanaged
	{
		if (this.Material is InstanceMaterialBase<TDataType> instanceMaterial)
		{
			return ref instanceMaterial.GetInstanceData(this);
		}

		throw new Exception("Material was not an instance material");
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.IsVisible)
			return;

		Transform thisTransform = this.Transform * transform;
		this.Data.Transform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		base.Draw(thisTransform, device, deviceContext);

		if (this.Material == null || this.materialException != null)
			return;

		if (this.Mesh == null)
			return;

		Mesh mesh = this.Mesh.Get();

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
			Vertex[] vertices = mesh.Vertices.ToArray();
			this.vertexLength = vertices.Length;
			this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, vertices);
			this.vertexBufferBinding = new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0);

			if (mesh.Indices != null)
			{
				ushort[] indices = mesh.Indices.ToArray();
				this.indexLength = indices.Length;
				this.indices = Buffer.Create(device, BindFlags.IndexBuffer, indices);
			}
		}

		deviceContext.InputAssembler.PrimitiveTopology = mesh.Topology;
		deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
		deviceContext.InputAssembler.SetIndexBuffer(this.indices, SharpDX.DXGI.Format.R16_UInt, 0);

		this.Material.Bind(this, device, deviceContext);

		if (this.indices == null)
		{
			deviceContext.Draw(this.vertexLength, 0);
		}
		else
		{
			deviceContext.DrawIndexed(this.indexLength, 0, 0);
		}
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, HitTestResult result)
	{
		if (!this.IsHitTestVisible)
			return;

		Transform thisTransform = this.Transform * transform;

		if (this.Mesh != null)
		{
			Mesh mesh = this.Mesh.Get();

			// Very basic 'closest vert' hit testing.
			// TODO: Start checking line and face intersections based on the mesh topology?
			for (int i = 0; i < mesh.Vertices.Count; i += 2)
			{
				Vector4 vertPos = mesh.Vertices[i].Position;
				vertPos = Vector4.Transform(vertPos, thisTransform.ToMatrix());
				vertPos = viewProjection.TransformViewProjection(vertPos);

				float fromDist = (screenPosition - vertPos.AsVector2()).Length();
				if (fromDist < result.Distance)
				{
					result.MeshVertex = mesh.Vertices[i];
					result.Distance = fromDist;
					result.SceneObject = this;
				}
			}
		}
	}

	public override void Dispose()
	{
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct PerRendererData
	{
		public Matrix4x4 Transform;
		public Color Color;
	}
}
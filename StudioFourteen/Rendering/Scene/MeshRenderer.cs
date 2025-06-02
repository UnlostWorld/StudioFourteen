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

using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using StudioFourteen.Content;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

[StructLayout(LayoutKind.Sequential)]
public struct MeshRendererInstanceData
{
	public Matrix4x4 Transform;
}

public class MeshRenderer<TMaterialData> : InstanceRendererBase<MeshRendererInstanceData, TMaterialData>
	where TMaterialData : unmanaged, IMaterial
{
	public IContent<Mesh>? Mesh;

	public float HitTestBias = 0;

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;
	private Buffer? indices;
	private int indexLength = 0;

	public MeshRenderer()
	{
	}

	public MeshRenderer(IContent<Mesh> mesh)
	{
		this.Mesh = mesh;
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.IsVisible)
			return;

		Transform thisTransform = this.Transform * transform;
		this.Instance.Transform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		base.Draw(thisTransform, device, deviceContext);

		if (this.Mesh == null)
			return;

		Mesh mesh = this.Mesh.Get();

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
				if (fromDist > result.MaxDistance)
					continue;

				fromDist -= this.HitTestBias / 100;
				fromDist -= vertPos.Z * 10;

				if (fromDist < result.Distance)
				{
					result.MeshVertex = mesh.Vertices[i];
					result.Distance = fromDist;
					result.SceneObject = this;
					result.Depth = vertPos.Z;

					Vector4 vertNormal = mesh.Vertices[i].Normal;
					vertNormal = Vector4.Transform(vertNormal, thisTransform.ToMatrix());
					vertNormal = viewProjection.TransformViewProjection(vertNormal);
					result.ScreenNormal = Vector2.Normalize(vertNormal.AsVector2());
				}
			}
		}
	}

	public override void Dispose()
	{
		this.vertices?.Dispose();
		this.indices?.Dispose();
	}
}
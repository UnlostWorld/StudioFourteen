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

namespace StudioFourteen.Rendering.Draw;

using System.Numerics;
using SharpDX.Direct3D11;
using StudioFourteen.Utilities;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

public class LineRenderer<TMaterialData> : InstanceRendererBase<MeshRendererInstanceData, TMaterialData>
	where TMaterialData : unmanaged, IMaterial
{
	public float HitTestBias = 0;

	private readonly Vertex[] vertArray = new Vertex[2]
	{
		new Vertex(Vector4.One, Color.White),
		new Vertex(Vector4.One, Color.White),
	};

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;

	public Vector3 From
	{
		get => this.vertArray[0].Position.AsVector3();
		set => this.vertArray[0].Position = new(value, 1.0f);
	}

	public Vector3 To
	{
		get => this.vertArray[1].Position.AsVector3();
		set => this.vertArray[1].Position = new(value, 1.0f);
	}

	public override void Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.IsVisible)
			return;

		Transform thisTransform = transform * this.Transform;
		this.Instance.Transform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		base.Draw(renderer, thisTransform, device, deviceContext);

		if (this.vertices == null)
		{
			this.vertexLength = 2;
			this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, this.vertArray);
			this.vertexBufferBinding = new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0);
		}

		deviceContext.UpdateSubresource(this.vertArray, this.vertices);

		deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
		deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);
		deviceContext.Draw(this.vertexLength, 0);
	}

	public override void Dispose()
	{
		this.vertices?.Dispose();
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, HitTestResult result)
	{
		if (!this.IsHitTestVisible)
			return;

		Transform thisTransform = this.Transform * transform;

		Vector4 fromPos = this.vertArray[0].Position;
		fromPos = Vector4.Transform(fromPos, thisTransform.ToMatrix());
		fromPos = viewProjection.TransformViewProjection(fromPos);

		Vector4 toPos = this.vertArray[1].Position;
		toPos = Vector4.Transform(toPos, thisTransform.ToMatrix());
		toPos = viewProjection.TransformViewProjection(toPos);

		Vector2 nearestPoint = MathUtility.FindNearestPointOnLine(fromPos.AsVector2(), toPos.AsVector2(), screenPosition);

		float fromDist = (screenPosition - nearestPoint).Length();
		if (fromDist > result.MaxDistance)
			return;

		float zDepth = fromPos.Z;

		fromDist -= this.HitTestBias / 100;
		fromDist -= zDepth * 10;

		if (fromDist < result.Distance)
		{
			result.Distance = fromDist;
			result.SceneObject = this;
			result.Depth = zDepth;
			result.ScreenNormal = Vector2.Normalize(toPos.AsVector2() - fromPos.AsVector2());
		}
	}
}
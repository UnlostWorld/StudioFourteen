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
using StudioFourteen.Rendering.Materials;

using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

public class LineRenderer : InstanceRendererBase<MeshRenderer.PerRendererData>
{
	public MaterialBase? Material;

	private readonly Vertex[] vertArray = new Vertex[2]
	{
		new Vertex(Vector4.One, Color.White),
		new Vertex(Vector4.One, Color.White),
	};

	private Buffer? vertices;
	private VertexBufferBinding vertexBufferBinding;
	private int vertexLength = 0;

	private Exception? materialException;

	public LineRenderer()
	{
		this.Color = Color.White;
	}

	public LineRenderer(MaterialBase material)
	{
		this.Material = material;
		this.Color = Color.White;
	}

	public Color Color
	{
		get => this.Data.Color;
		set => this.Data.Color = value;
	}

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
		Transform thisTransform = transform * this.Transform;
		this.Data.Transform = Matrix4x4.Transpose(thisTransform.ToMatrix());
		base.Draw(thisTransform, device, deviceContext);

		if (this.Material == null || this.materialException != null)
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
			this.vertexLength = 2;
			this.vertices = Buffer.Create(device, BindFlags.VertexBuffer, this.vertArray);
			this.vertexBufferBinding = new VertexBufferBinding(this.vertices, SharpDX.Utilities.SizeOf<Vertex>(), 0);
		}

		deviceContext.UpdateSubresource(this.vertArray, this.vertices);

		deviceContext.InputAssembler.PrimitiveTopology = SharpDX.Direct3D.PrimitiveTopology.LineList;
		deviceContext.InputAssembler.SetVertexBuffers(0, this.vertexBufferBinding);

		this.Material.Bind(this, device, deviceContext);
		deviceContext.Draw(this.vertexLength, 0);
	}

	public override void HitTest(Vector2 screenPosition, Transform transform, Transform viewProjection, ref HitTestResult result)
	{
	}

	public override void Dispose()
	{
		this.vertices?.Dispose();
	}
}
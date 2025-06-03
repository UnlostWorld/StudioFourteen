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
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using StudioFourteen.Content;
using System.Collections.Generic;

using Buffer = SharpDX.Direct3D11.Buffer;

public interface IMaterial
{
	IContent<ShaderBytecode>? GetVertexShader();
	IContent<ShaderBytecode>? GetPixelShader();
	IContent<ShaderBytecode>? GetGeometryShader();

	void Initialize();
}

public abstract class InstanceRendererBase<TRendererData, TMaterialData> : RendererBase
	where TRendererData : unmanaged
	where TMaterialData : unmanaged, IMaterial
{
	public TRendererData Instance;
	public TMaterialData Material;

	private Buffer? rendererDataBuffer;
	private Buffer? materialDataBuffer;

	private Shader? shader;
	private InputLayout? layout;

	public InstanceRendererBase()
	{
		this.Material.Initialize();
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (this.rendererDataBuffer == null)
		{
			this.rendererDataBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<TRendererData>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		if (this.materialDataBuffer == null)
		{
			int size = SharpDX.Utilities.SizeOf<TMaterialData>();

			if (size == 0)
				throw new Exception($"{typeof(TMaterialData)} size is 0. Ensure there are at least 16 bytes occupied (one Vector4)");

			if (size % 16 != 0)
				throw new Exception($"{typeof(TMaterialData)} is {size} bytes. Not divisible by 16, ensure struct is packed in sets of 4 floats.");

			this.materialDataBuffer = new(
				device,
				size,
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		if (this.shader == null)
		{
			this.shader = this.Services.Rendering.ShaderCache.GetShader<TMaterialData>(device);

			if (this.shader == null)
				return;

			if (this.shader.VertexSignature != null)
			{
				this.layout = new InputLayout(device, this.shader.VertexSignature, default(Vertex).GetInputElements());
			}
		}

		if (this.layout != null)
			deviceContext.InputAssembler.InputLayout = this.layout;

		deviceContext.VertexShader.Set(this.shader.Vertex);
		deviceContext.VertexShader.SetConstantBuffer(Registers.PerRendererData, this.rendererDataBuffer);
		deviceContext.VertexShader.SetConstantBuffer(Registers.PerMaterialData, this.materialDataBuffer);

		deviceContext.GeometryShader.Set(this.shader.Geometry);
		deviceContext.GeometryShader.SetConstantBuffer(Registers.PerRendererData, this.rendererDataBuffer);
		deviceContext.GeometryShader.SetConstantBuffer(Registers.PerMaterialData, this.materialDataBuffer);

		deviceContext.PixelShader.Set(this.shader.Pixel);
		deviceContext.PixelShader.SetConstantBuffer(Registers.PerRendererData, this.rendererDataBuffer);
		deviceContext.PixelShader.SetConstantBuffer(Registers.PerMaterialData, this.materialDataBuffer);

		// TODO: Use a buffer array and an index instead of updating every draw call?
		deviceContext.UpdateSubresource(ref this.Instance, this.rendererDataBuffer);
		deviceContext.UpdateSubresource(ref this.Material, this.materialDataBuffer);
	}

	public override void Dispose()
	{
		this.layout?.Dispose();
	}
}
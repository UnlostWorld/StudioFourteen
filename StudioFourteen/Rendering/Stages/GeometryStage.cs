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

namespace StudioFourteen.Rendering.Stages;

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Rendering.Geometry;
using Device = SharpDX.Direct3D11.Device;

public class GeometryStage : RenderStageBase
{
	private readonly List<RenderableObject> renderables = new();
	private Buffer? constantsBuffer;
	private Constants constants;
	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;

	public GeometryStage()
	{
		////this.renderables.Add(new(new("VertexColor.hlsl"), new FlatCubeGeometry()));
	}

	public void Add(RenderableObject obj)
	{
		lock(this.renderables)
		{
			this.renderables.Add(obj);
		}
	}

	public void Remove(RenderableObject obj)
	{
		lock(this.renderables)
		{
			this.renderables.Remove(obj);
		}
	}

	public override void Render(RenderingService service, Device device, DeviceContext deviceContext)
	{
		if (this.backBufferTargetView == null)
		{
			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			desc.Texture2D = new() { };
			this.backBufferTargetView = new(device, service.BackBuffer, desc);
		}

		if (this.constantsBuffer == null)
		{
			this.constantsBuffer = new(
				device,
				SharpDX.Utilities.SizeOf<Constants>(),
				ResourceUsage.Default,
				BindFlags.ConstantBuffer,
				CpuAccessFlags.None,
				ResourceOptionFlags.None,
				0);
		}

		if (this.blend == null)
		{
			BlendStateDescription blendDesc = default;
			blendDesc.AlphaToCoverageEnable = false;
			blendDesc.RenderTarget[0].IsBlendEnabled = true;
			blendDesc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
			blendDesc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
			blendDesc.RenderTarget[0].BlendOperation = BlendOperation.Add;
			blendDesc.RenderTarget[0].SourceAlphaBlend = BlendOption.Zero;
			blendDesc.RenderTarget[0].DestinationAlphaBlend = BlendOption.Zero;
			blendDesc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
			blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

			this.blend = new(device, blendDesc);
		}

		this.constants.ViewProjection = Matrix4x4.Transpose(service.Services.Camera.CurrentViewProjection);

		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);
		deviceContext.OutputMerger.SetBlendState(this.blend, null, -1);
		deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);
		deviceContext.VertexShader.SetConstantBuffer(0, this.constantsBuffer);
		deviceContext.GeometryShader.SetConstantBuffer(0, this.constantsBuffer);

		lock(this.renderables)
		{
			foreach(RenderableObject renderable in this.renderables)
			{
				this.constants.ObjectTransform = Matrix4x4.Transpose(renderable.Transform.ToMatrix());
				deviceContext.UpdateSubresource(ref this.constants, this.constantsBuffer);
				renderable.Draw(service, device, deviceContext);
			}
		}

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();
	}

	public override void Dispose()
	{
		this.backBufferTargetView?.Dispose();
		this.backBufferTargetView = null;
		this.constantsBuffer?.Dispose();
		this.constantsBuffer = null;

		foreach(RenderableObject renderable in this.renderables)
		{
			renderable.Dispose();
		}

		base.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Constants
	{
		public Matrix4x4 ViewProjection;
		public Matrix4x4 ObjectTransform;
	}
}
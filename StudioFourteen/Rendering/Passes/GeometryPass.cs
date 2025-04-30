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

namespace StudioFourteen.Rendering.Passes;

using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

public class GeometryPass : InstanceRenderPassBase<GeometryPass.GeometryPassData>
{
	private readonly List<RendererBase> renderables = new();

	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;

	public void Add(RendererBase obj)
	{
		lock(this.renderables)
		{
			this.renderables.Add(obj);
		}
	}

	public void Remove(RendererBase obj)
	{
		lock(this.renderables)
		{
			this.renderables.Remove(obj);
		}
	}

	public void HitTest(Vector2 screenPosition, ref HitTestResult result)
	{
		Matrix4x4 viewProj = ServiceManager.Instance.Camera.CurrentViewProjection;

		foreach(RendererBase draw in this.renderables)
		{
			draw.HitTest(screenPosition, Transform.Identity, viewProj, ref result);
		}
	}

	public override void Render(RenderingService service, Device device, DeviceContext deviceContext)
	{
		this.PassData.ViewProjection = Matrix4x4.Transpose(service.Services.Camera.CurrentViewProjection);
		this.PassData.CameraPosition = new Vector4(service.Services.Camera.CurrentPosition, 1);

		base.Render(service, device, deviceContext);

		if (this.backBufferTargetView == null)
		{
			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			desc.Texture2D = new() { };
			this.backBufferTargetView = new(device, service.BackBuffer, desc);
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

		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);
		deviceContext.OutputMerger.SetBlendState(this.blend, null, -1);
		deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);

		lock(this.renderables)
		{
			foreach(RendererBase renderable in this.renderables)
			{
				renderable.Draw(Transform.Identity, device, deviceContext);
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

		foreach(RendererBase renderable in this.renderables)
		{
			renderable.Dispose();
		}

		base.Dispose();
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GeometryPassData
	{
		public Matrix4x4 ViewProjection;
		public Vector4 CameraPosition;
	}
}
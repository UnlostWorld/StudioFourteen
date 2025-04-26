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
using SharpDX.Direct3D11;
using SharpDX.DXGI;

using Device = SharpDX.Direct3D11.Device;

public class GeometryPass : RenderPassBase
{
	private readonly List<DrawBase> renderables = new();
	private readonly DrawState drawState = new(0);

	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;

	public void Add(DrawBase obj)
	{
		lock(this.renderables)
		{
			this.renderables.Add(obj);
		}
	}

	public void Remove(DrawBase obj)
	{
		lock(this.renderables)
		{
			this.renderables.Remove(obj);
		}
	}

	public void HitTest(Vector2 screenPosition, ref HitTestResult result)
	{
		Matrix4x4 viewProj = ServiceManager.Instance.Camera.CurrentViewProjection;

		foreach(DrawBase draw in this.renderables)
		{
			draw.HitTest(screenPosition, Transform.Identity, viewProj, ref result);
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

		this.drawState.Data.ClippingPlanes.X = service.Services.Camera.NearPlane;
		this.drawState.Data.ClippingPlanes.Y = service.Services.Camera.FarPlane;
		this.drawState.Data.ViewProjection = Matrix4x4.Transpose(service.Services.Camera.CurrentViewProjection);
		this.drawState.Bind(device, deviceContext);

		deviceContext.Rasterizer.SetViewport(0, 0, service.Width, service.Height);
		deviceContext.OutputMerger.SetBlendState(this.blend, null, -1);
		deviceContext.OutputMerger.SetTargets(this.backBufferTargetView);

		lock(this.renderables)
		{
			foreach(DrawBase renderable in this.renderables)
			{
				renderable.Draw(Transform.Identity, this.drawState);
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

		this.drawState.Dispose();

		foreach(DrawBase renderable in this.renderables)
		{
			renderable.Dispose();
		}

		base.Dispose();
	}
}
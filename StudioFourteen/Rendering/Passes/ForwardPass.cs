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

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using StudioFourteen.Rendering.Draw;

using Device = SharpDX.Direct3D11.Device;

public class ForwardPass : InstanceRenderPassBase<ForwardPass.ForwardPassData>
{
	private readonly List<DrawObject> sceneObjects = new();

	private RenderTargetView? backBufferTargetView;
	private BlendState? blend;

	private Texture2D? depthStencilTexture;
	private DepthStencilView? depthStencilView;
	private DepthStencilState? depthStencilState;

	public float ViewportScale { get; set; } = 1;

	public void Add(DrawObject obj)
	{
		lock (this.sceneObjects)
		{
			if (this.sceneObjects.Contains(obj))
				return;

			this.sceneObjects.Add(obj);
		}
	}

	public void Remove(DrawObject obj)
	{
		lock(this.sceneObjects)
		{
			this.sceneObjects.Remove(obj);
		}
	}

	public void HitTest(Vector2 screenPosition, HitTestResult result)
	{
		Matrix4x4 viewProj = this.Services.Camera.CurrentView * this.Services.Camera.CurrentProjection;

		lock (this.sceneObjects)
		{
			foreach (DrawObject draw in this.sceneObjects)
			{
				if (!draw.IsHitTestVisible)
					continue;

				try
				{
					draw.HitTest(screenPosition, Transform.Identity, viewProj, result);
				}
				catch (Exception ex)
				{
					draw.IsHitTestVisible = false;
					this.Log.Error(ex, $"Error hit testing forward object: {draw}. This object will be disabled.");
				}
			}
		}
	}

	public override void OnResolutionChanging()
	{
		this.backBufferTargetView?.Dispose();
		this.backBufferTargetView = null;

		base.OnResolutionChanging();
	}

	public override void Dispose()
	{
		this.backBufferTargetView?.Dispose();
		this.backBufferTargetView = null;

		this.depthStencilTexture?.Dispose();
		this.depthStencilTexture = null;

		this.depthStencilView?.Dispose();
		this.depthStencilView = null;

		this.depthStencilState?.Dispose();
		this.depthStencilState = null;

		foreach(DrawObject renderable in this.sceneObjects)
		{
			renderable.Dispose();
		}

		base.Dispose();
	}

	public override void Render(Renderer renderer, Device device, DeviceContext deviceContext)
	{
		this.PassData.ViewMatrix = Matrix4x4.Transpose(renderer.Camera.GetViewMatrix(renderer));
		this.PassData.ProjectionMatrix = Matrix4x4.Transpose(renderer.Camera.GetProjectionMatrix(renderer));
		this.PassData.CameraPosition = new Vector4(renderer.Camera.GetCameraPosition(renderer), 1);
		this.PassData.ViewportScale = (1 + (1 - (renderer.Width / 1024))) * this.ViewportScale;

		base.Render(renderer, device, deviceContext);

		if (this.backBufferTargetView == null)
		{
			this.backBufferTargetView?.Dispose();

			RenderTargetViewDescription desc = default;
			desc.Format = Format.R8G8B8A8_UNorm;
			desc.Dimension = RenderTargetViewDimension.Texture2D;
			desc.Texture2D = new() { };

			this.backBufferTargetView = new(device, renderer.BackBuffer, desc);
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
			blendDesc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.Red | ColorWriteMaskFlags.Green | ColorWriteMaskFlags.Blue;

			this.blend = new(device, blendDesc);
		}

		if (this.depthStencilTexture == null)
		{
			Texture2DDescription desc = default;
			desc.Format = Format.D24_UNorm_S8_UInt;
			desc.ArraySize = 1;
			desc.MipLevels = 1;
			desc.Width = (int)renderer.Width;
			desc.Height = (int)renderer.Height;
			desc.SampleDescription = new SampleDescription(1, 0);
			desc.Usage = ResourceUsage.Default;
			desc.BindFlags = BindFlags.DepthStencil;
			desc.CpuAccessFlags = CpuAccessFlags.None;
			desc.OptionFlags = ResourceOptionFlags.None;

			this.depthStencilTexture = new(device, desc);
		}

		if (this.depthStencilView == null)
		{
			this.depthStencilView = new(device, this.depthStencilTexture);
		}

		if (this.depthStencilState == null)
		{
			DepthStencilStateDescription desc = DepthStencilStateDescription.Default();
			desc.DepthComparison = Comparison.GreaterEqual;
			desc.DepthWriteMask = DepthWriteMask.All;
			desc.IsDepthEnabled = true;
			desc.IsStencilEnabled = true;

			this.depthStencilState = new(device, desc);
		}

		deviceContext.Rasterizer.SetViewport(0, 0, renderer.Width, renderer.Height);
		deviceContext.OutputMerger.SetBlendState(this.blend, null, -1);
		deviceContext.OutputMerger.SetTargets(this.depthStencilView, this.backBufferTargetView);
		deviceContext.OutputMerger.SetDepthStencilState(this.depthStencilState, int.MinValue);

		deviceContext.ClearDepthStencilView(this.depthStencilView, DepthStencilClearFlags.Depth, 0f, byte.MaxValue);

		lock (this.sceneObjects)
		{
			foreach (DrawObject drawObject in this.sceneObjects)
			{
				drawObject.Draw(renderer, Transform.Identity, device, deviceContext);
			}
		}

		using CommandList cmds = deviceContext.FinishCommandList(false);
		device.ImmediateContext.ExecuteCommandList(cmds, true);
		deviceContext.ClearState();

		////device.ImmediateContext.ClearRenderTargetView(this.backBufferTargetView, new(1, 1, 0, 1));
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct ForwardPassData
	{
		public Matrix4x4 ViewMatrix;
		public Matrix4x4 ProjectionMatrix;
		public Vector4 CameraPosition;
		public float ViewportScale;
		public float Unused1;
		public float Unused2;
		public float Unused3;
	}
}
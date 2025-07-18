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

namespace StudioFourteen.Rendering.WPF;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Services;

using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11Resource = SharpDX.Direct3D11.Resource;
using D3D11Texture = SharpDX.Direct3D11.Texture2D;
using D3D9Device = SharpDX.Direct3D9.DeviceEx;
using D3D9Format = SharpDX.Direct3D9.Format;
using D3D9Pool = SharpDX.Direct3D9.Pool;
using D3D9PresentParameters = SharpDX.Direct3D9.PresentParameters;
using D3D9Resource = SharpDX.Direct3D9.Resource;
using D3D9Surface = SharpDX.Direct3D9.Surface;
using D3D9Texture = SharpDX.Direct3D9.Texture;
using D3D9Usage = SharpDX.Direct3D9.Usage;
using DXGIFormat = SharpDX.DXGI.Format;
using DXGIResource = SharpDX.DXGI.Resource;
using DXGISwapChain = SharpDX.DXGI.SwapChain;
using DXGISwapEffect = SharpDX.DXGI.SwapEffect;
using DXGIUsage = SharpDX.DXGI.Usage;

public partial class RendererElement : Image
{
	private WpfRenderer? renderer;

	public RendererElement()
	{
		this.Loaded += this.OnLoaded;
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);

		if (this.renderer == null || this.ActualWidth <= 0 || this.ActualHeight <= 0)
			return;

	////	this.renderer.NewWidth = (int)this.ActualWidth;
	////	this.renderer.NewHeight = (int)this.ActualHeight;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Window? wnd = this.FindParent<Window>();
		if (wnd == null)
			return;

		WindowInteropHelper interop = new(wnd);

		this.renderer = new(interop.Handle);
		this.Source = this.renderer.Source;
	}
}

public class WpfRenderer : Renderer
{
	public readonly ForwardPass Forward = new();
	private readonly MeshRenderer<BlitMaterial> quad = new(MeshContent.Quad);
	private readonly nint windowHandle;
	private D3D11Texture? backBuffer;

	private D3D9Texture? d3d9SharedTexture;
	private D3D11Texture? d3d11SharedTexture;
	private D3D9Device? d3d9Device;
	private D3D11Device? d3d11Device;
	private DXGISwapChain? swapChain;
	private ShaderResourceView? backSrv;
	private RenderTargetView? backRtv;
	private RenderTargetView? outputRtv;
	private D3D9Surface? d3d9Surface;

	private ShaderResourceView? maskResourceView;
	private ShaderResourceView? depthResourceView;

	public WpfRenderer(nint windowHandle)
	{
		this.windowHandle = windowHandle;
		this.AddPass(this.Forward);

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		GridGizmo gg = new();
		this.Forward.Add(gg);
	}

	public D3DImage Source { get; init; } = new();

	public override void Dispose()
	{
		this.d3d9Device?.Dispose();
		this.d3d9Device = null;

		this.d3d9SharedTexture?.Dispose();
		this.d3d9SharedTexture = null;

		this.d3d11SharedTexture?.Dispose();
		this.d3d11SharedTexture = null;

		this.d3d11Device?.Dispose();
		this.d3d11Device = null;

		this.swapChain?.Dispose();
		this.d3d11Device = null;

		this.outputRtv?.Dispose();
		this.outputRtv = null;

		this.d3d9Surface?.Dispose();
		this.d3d9Surface = null;

		this.backSrv?.Dispose();
		this.backSrv = null;

		this.maskResourceView?.Dispose();
		this.maskResourceView = null;

		this.depthResourceView?.Dispose();
		this.depthResourceView = null;

		base.Dispose();
	}

	public override void Render()
	{
		if (this.backRtv != null && this.d3d11Device != null)
			this.d3d11Device.ImmediateContext.ClearRenderTargetView(this.backRtv, new(0, 0, 0, 0));

		base.Render();

		if (this.d3d9Surface == null || this.outputRtv == null || this.d3d11Device == null)
			return;

		this.d3d11Device.ImmediateContext.PixelShader.SetShaderResource(2, this.backSrv);
		this.d3d11Device.ImmediateContext.OutputMerger.SetTargets(this.outputRtv);
		this.d3d11Device.ImmediateContext.Rasterizer.SetViewport(0, 0, this.Width, this.Height);

		this.quad.Draw(Transform.Identity, this.d3d11Device, this.d3d11Device.ImmediateContext);

		this.Source.Dispatcher.Invoke(() =>
		{
			this.Source.Lock();
			this.Source.AddDirtyRect(new Int32Rect(0, 0, this.Source.PixelWidth, this.Source.PixelHeight));
			this.Source.Unlock();
		});
	}

	protected override void RenderPass(RenderPassBase pass)
	{
		this.DeviceContext?.PixelShader.SetShaderResource(0, this.maskResourceView);
		this.DeviceContext?.PixelShader.SetShaderResource(1, this.depthResourceView);

		base.RenderPass(pass);
	}

	protected override D3D11Texture? GetBackBuffer()
	{
		if (this.backBuffer == null)
		{
			// Create a D3D11 device and swapchain to handle the rendering.
			ModeDescription backBufferDesc = new(
				(int)this.Width,
				(int)this.Height,
				new Rational(60, 1),
				DXGIFormat.R8G8B8A8_UNorm);

			SwapChainDescription swapChainDesc = new SwapChainDescription()
			{
				ModeDescription = backBufferDesc,
				SampleDescription = new SampleDescription(1, 0),
				Usage = DXGIUsage.RenderTargetOutput | DXGIUsage.BackBuffer | DXGIUsage.Shared | DXGIUsage.ShaderInput,
				BufferCount = 1,
				IsWindowed = true,
				SwapEffect = DXGISwapEffect.Discard,
				Flags = SwapChainFlags.None,
				OutputHandle = this.windowHandle,
			};

			D3D11Device.CreateWithSwapChain(
				DriverType.Hardware,
				DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
				[FeatureLevel.Level_11_1, FeatureLevel.Level_9_3],
				swapChainDesc,
				out var d3d11Device,
				out var swapChain);

			this.d3d11Device = d3d11Device;
			this.swapChain = swapChain;

			this.backBuffer = D3D11Resource.FromSwapChain<D3D11Texture>(swapChain, 0);
			if (this.backBuffer == null)
				throw new Exception("Failed to create back buffer");

			this.backSrv = new(this.d3d11Device, this.backBuffer);
			this.backRtv = new(this.d3d11Device, this.backBuffer);

			// D3D9 output
			D3D9PresentParameters presentparams = new()
			{
				Windowed = true,
				SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
				DeviceWindowHandle = this.windowHandle,
				PresentationInterval = PresentInterval.Default,
			};

			const CreateFlags deviceFlags = CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve;

			Direct3DEx direct3d = new();
			this.d3d9Device = new(direct3d, 0, DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentparams);

			nint sharedHandle = 0;
			this.d3d9SharedTexture = new(
				this.d3d9Device,
				this.Width,
				this.Height,
				1,
				D3D9Usage.RenderTarget,
				D3D9Format.A8R8G8B8,
				D3D9Pool.Default,
				ref sharedHandle);

			this.d3d9Surface = this.d3d9SharedTexture.GetSurfaceLevel(0);

			this.d3d11SharedTexture = d3d11Device.OpenSharedResource<D3D11Texture>(sharedHandle);
			this.outputRtv = new(d3d11Device, this.d3d11SharedTexture);

			this.Source.Dispatcher.Invoke(() =>
			{
				this.Source.Lock();
				this.Source.SetBackBuffer(D3DResourceType.IDirect3DSurface9, this.d3d9Surface.NativePointer);
				this.Source.AddDirtyRect(new Int32Rect(0, 0, this.Source.PixelWidth, this.Source.PixelHeight));
				this.Source.Unlock();
			});
		}

		return this.backBuffer;
	}

	private void OnGameTick()
	{
		this.Render();
	}
}
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using DependencyPropertyGenerator;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using StudioFourteen.Rendering.Passes;

using D3D11Resource = SharpDX.Direct3D11.Resource;
using D3D9Device = SharpDX.Direct3D9.DeviceEx;
using D3D9Format = SharpDX.Direct3D9.Format;
using D3D9Pool = SharpDX.Direct3D9.Pool;
using D3D9PresentParameters = SharpDX.Direct3D9.PresentParameters;
using D3D9Resource = SharpDX.Direct3D9.Resource;
using D3D9Surface = SharpDX.Direct3D9.Surface;
using D3D9Texture = SharpDX.Direct3D9.Texture;
using D3D9Usage = SharpDX.Direct3D9.Usage;
using Device = SharpDX.Direct3D11.Device;
using DXGIFormat = SharpDX.DXGI.Format;
using DXGIResource = SharpDX.DXGI.Resource;
using DXGISwapEffect = SharpDX.DXGI.SwapEffect;
using DXGIUsage = SharpDX.DXGI.Usage;

public partial class RendererElement : Image
{
	private WpfRenderer? renderer;

	public RendererElement()
	{
		this.Loaded += this.OnLoaded;
	}

	protected override void OnRender(DrawingContext dc)
	{
		this.renderer?.Render();
		base.OnRender(dc);
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);

		if (this.renderer == null || this.ActualWidth <= 0 || this.ActualHeight <= 0)
			return;

		this.renderer.NewWidth = (int)this.ActualWidth;
		this.renderer.NewHeight = (int)this.ActualHeight;
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
	private readonly nint windowHandle;
	private Texture2D? backBuffer;

	public WpfRenderer(nint windowHandle)
	{
		this.windowHandle = windowHandle;
		this.AddPass(this.Forward);
	}

	public D3DImage Source { get; init; } = new();

	protected override Texture2D? GetBackBuffer()
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
				Usage = DXGIUsage.RenderTargetOutput | DXGIUsage.BackBuffer | DXGIUsage.Shared,
				BufferCount = 1,
				IsWindowed = true,
				SwapEffect = DXGISwapEffect.Discard,
				Flags = SwapChainFlags.None,
				OutputHandle = this.windowHandle,
			};

			Device.CreateWithSwapChain(
				DriverType.Hardware,
				DeviceCreationFlags.BgraSupport | DeviceCreationFlags.Debug,
				[FeatureLevel.Level_11_1, FeatureLevel.Level_9_3],
				swapChainDesc,
				out var d3d11Device,
				out var swapChain);

			this.backBuffer = D3D11Resource.FromSwapChain<Texture2D>(swapChain, 0);
			if (this.backBuffer == null)
				throw new Exception("Failed to create back buffer");

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
			D3D9Device d3d9Device = new(direct3d, 0, DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentparams);

			D3D9Texture d3d9Texture = new(
				d3d9Device,
				(int)this.Width,
				(int)this.Height,
				1,
				D3D9Usage.RenderTarget,
				D3D9Format.A8R8G8B8,
				D3D9Pool.Default);

			using D3D9Surface surface = d3d9Texture.GetSurfaceLevel(0);
			d3d9Device.ColorFill(surface, new RawColorBGRA(255, 255, 255, 255));

			// TODO: copy the dx11 texture contents into the Dx9 texture, oh boy!
			////using DXGIResource resource = this.backBuffer.QueryInterface<DXGIResource>();
			////nint handle = resource.SharedHandle;
			////D3D9Resource resource9 = new(d3d9Texture.NativePointer);
			////D3D11Resource resource11 = new(resource9.NativePointer);
			////RenderTargetView rtv = new(d3d11Device, resource11);*/

			this.Source.Lock();
			this.Source.SetBackBuffer(D3DResourceType.IDirect3DSurface9, surface.NativePointer);
			this.Source.AddDirtyRect(new Int32Rect(0, 0, this.Source.PixelWidth, this.Source.PixelHeight));
			this.Source.Unlock();
		}

		return null;
	}
}
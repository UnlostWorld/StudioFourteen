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
using System.Numerics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using DependencyPropertyGenerator;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.DXGI;
using StudioFourteen.Cursors;
using StudioFourteen.Input;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using D3D11Device = SharpDX.Direct3D11.Device;
using D3D11Resource = SharpDX.Direct3D11.Resource;
using D3D11Texture = SharpDX.Direct3D11.Texture2D;
using D3D9Device = SharpDX.Direct3D9.DeviceEx;
using D3D9Format = SharpDX.Direct3D9.Format;
using D3D9Pool = SharpDX.Direct3D9.Pool;
using D3D9PresentParameters = SharpDX.Direct3D9.PresentParameters;
using D3D9Surface = SharpDX.Direct3D9.Surface;
using D3D9Texture = SharpDX.Direct3D9.Texture;
using D3D9Usage = SharpDX.Direct3D9.Usage;
using DXGIFormat = SharpDX.DXGI.Format;
using DXGISwapChain = SharpDX.DXGI.SwapChain;
using DXGISwapEffect = SharpDX.DXGI.SwapEffect;
using DXGIUsage = SharpDX.DXGI.Usage;

[DependencyProperty<int>("RenderWidth", DefaultValue = 256)]
[DependencyProperty<int>("RenderHeight", DefaultValue = 256)]
public abstract partial class RendererElement : Image
{
	private readonly HitTestResult pressHitTestResult = new();

	public RendererElement()
	{
		this.Stretch = System.Windows.Media.Stretch.Uniform;
		this.Loaded += this.OnLoaded;
	}

	protected WpfRenderer? Renderer { get; private set; }
	protected abstract RendererCamera Camera { get; }

	protected abstract void Initialize();

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		Window? wnd = this.FindParent<Window>();
		if (wnd == null)
			return;

		WindowInteropHelper interop = new(wnd);
		WpfInput input = new(this);

		this.Renderer = new(interop.Handle, this.Camera, input);
		this.Source = this.Renderer.Source;
		this.Renderer.NewWidth = this.RenderWidth;
		this.Renderer.NewHeight = this.RenderHeight;

		this.Initialize();
	}

	partial void OnRenderHeightChanged(int newValue)
	{
		if (this.Renderer == null)
			return;

		this.Renderer.NewHeight = newValue;
	}

	partial void OnRenderWidthChanged(int newValue)
	{
		if (this.Renderer == null)
			return;

		this.Renderer.NewWidth = newValue;
	}
}

public class WpfInput : RendererInput
{
	private InputStates mouseState = InputStates.None;
	private Vector2 mousePosition = Vector2.Zero;
	private Vector2 dragDelta = Vector2.Zero;
	private Point startPosition;

	public WpfInput(UIElement element)
	{
		element.MouseDown += this.OnMouseDown;
		element.MouseUp += this.OnMouseUp;
		element.MouseLeave += this.OnMouseLeave;
		element.MouseMove += this.OnMouseMove;
	}

	protected void OnMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.startPosition = CursorUtility.GetPosition();
		this.mouseState = Input.InputStates.Activated;
		this.dragDelta = Vector2.Zero;

		FrameworkElement? element = sender as FrameworkElement;
		if (element == null)
			return;

		element.CaptureMouse();
		CursorUtility.SetCursorVisible(false);
	}

	protected void OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		this.mouseState = Input.InputStates.Deactivated;
		this.dragDelta = Vector2.Zero;

		FrameworkElement? element = sender as FrameworkElement;
		if (element == null)
			return;

		element.ReleaseMouseCapture();
		CursorUtility.SetCursorVisible(true);
	}

	protected void OnMouseLeave(object sender, MouseEventArgs e)
	{
		this.mouseState = Input.InputStates.None;
		this.mousePosition = Vector2.Zero;
		this.dragDelta = Vector2.Zero;
	}

	protected void OnMouseMove(object sender, MouseEventArgs e)
	{
		FrameworkElement? element = sender as FrameworkElement;
		if (element == null)
			return;

		this.mousePosition = e.GetPosition(element).ToVector2() / new Vector2((float)element.ActualWidth, (float)element.ActualHeight);

		if (this.mouseState == InputStates.Held)
		{
			Point newPos = CursorUtility.GetPosition();

			if (newPos != this.startPosition)
			{
				this.dragDelta = (this.startPosition.ToVector2() - newPos.ToVector2()) / 2.0f;
				CursorUtility.SetPosition(this.startPosition);
			}
		}
		else
		{
			this.dragDelta = Vector2.Zero;
		}
	}

	protected override void ProcessInput(
		out InputStates inputState,
		out Vector2? cursorPosition,
		out Vector2 dragDelta)
	{
		inputState = this.mouseState;
		cursorPosition = this.mousePosition;
		dragDelta = this.dragDelta;

		this.dragDelta = Vector2.Zero;

		if (this.mouseState == InputStates.Activated)
		{
			this.mouseState = InputStates.Held;
		}
		else if (this.mouseState == InputStates.Deactivated)
		{
			this.mouseState = InputStates.None;
		}
	}
}

public class WpfRenderer : Renderer
{
	public readonly ForwardPass Forward = new();
	private readonly MeshRenderer<BlitMaterial> quad = new(MeshContent.Quad);
	private readonly nint windowHandle;
	private readonly RendererCamera camera;
	private readonly RendererInput input;
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

	public WpfRenderer(nint windowHandle, RendererCamera camera, RendererInput input)
	{
		this.camera = camera;
		this.input = input;
		this.windowHandle = windowHandle;
		this.Forward.ViewportScale = 4;
		this.AddPass(this.Forward);

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public D3DImage Source { get; init; } = new();
	public override RendererCamera Camera => this.camera;
	public override RendererInput Input => this.input;

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

		if (!this.CanRender || this.d3d9Surface == null || this.outputRtv == null || this.d3d11Device == null)
			return;

		this.d3d11Device.ImmediateContext.PixelShader.SetShaderResource(2, this.backSrv);
		this.d3d11Device.ImmediateContext.OutputMerger.SetTargets(this.outputRtv);
		this.d3d11Device.ImmediateContext.Rasterizer.SetViewport(0, 0, this.Width, this.Height);

		this.quad.Draw(this, Transform.Identity, this.d3d11Device, this.d3d11Device.ImmediateContext);

		this.Source.Dispatcher.Invoke(() =>
		{
			this.Source.Lock();
			this.Source.AddDirtyRect(new Int32Rect(0, 0, this.Source.PixelWidth, this.Source.PixelHeight));
			this.Source.Unlock();
		});
	}

	protected override void RenderPass(RenderPassBase pass)
	{
		if (!this.CanRender)
			return;

		this.DeviceContext?.PixelShader.SetShaderResource(0, this.maskResourceView);
		this.DeviceContext?.PixelShader.SetShaderResource(1, this.depthResourceView);

		base.RenderPass(pass);
	}

	protected override void OnResolutionChanged()
	{
		this.backBuffer?.Dispose();
		this.backBuffer = null;

		this.swapChain?.Dispose();
		this.swapChain = null;

		this.d3d9SharedTexture?.Dispose();
		this.d3d9SharedTexture = null;

		this.d3d9Surface?.Dispose();
		this.d3d9Surface = null;

		this.d3d11SharedTexture?.Dispose();
		this.d3d11SharedTexture = null;

		this.backSrv?.Dispose();
		this.backSrv = null;

		this.backRtv?.Dispose();
		this.backRtv = null;

		base.OnResolutionChanged();
	}

	protected override D3D11Texture? GetBackBuffer()
	{
		if (this.d3d9Device == null)
		{
			const CreateFlags deviceFlags = CreateFlags.HardwareVertexProcessing | CreateFlags.FpuPreserve;

			// D3D9 output
			D3D9PresentParameters presentParameters = new()
			{
				Windowed = true,
				SwapEffect = SharpDX.Direct3D9.SwapEffect.Discard,
				DeviceWindowHandle = this.windowHandle,
				PresentationInterval = PresentInterval.Default,
			};

			Direct3DEx direct3d = new();
			this.d3d9Device = new(direct3d, 0, DeviceType.Hardware, IntPtr.Zero, deviceFlags, presentParameters);
		}

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

			this.d3d11Device?.Dispose();
			this.swapChain?.Dispose();

			D3D11Device.CreateWithSwapChain(
				DriverType.Hardware,
				DeviceCreationFlags.BgraSupport,
				[],
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
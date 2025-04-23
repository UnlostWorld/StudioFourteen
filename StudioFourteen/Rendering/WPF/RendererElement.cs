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

using System.Windows;
using System.Windows.Media;
using SharpDX.DXGI;
using SharpDX.Direct3D11;
using SharpDX.Direct3D;

using Device = SharpDX.Direct3D11.Device;

public class D3D11Surface : FrameworkElement
{
	private D3D11Image? image;

	public void SetBackbufferImage(D3D11Image image)
    {
        this.image = image;
        this.InvalidateVisual();
    }

	protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        if (this.image != null && this.image.IsFrontBufferAvailable)
		{
            drawingContext.DrawImage(this.image, new Rect(default, this.RenderSize));
		}
    }
}

public class RendererControl : D3D11Surface
{
	public int RenderingWidth = 1920;
	public int RenderingHeight = 1080;

	public RendererControl()
	{
		ModeDescription backBufferDesc = new ModeDescription(this.RenderingWidth, this.RenderingHeight, new Rational(60, 1), Format.R8G8B8A8_UNorm);

		SwapChainDescription swapChainDesc = new SwapChainDescription()
		{
		    ModeDescription = backBufferDesc,
		    SampleDescription = new SampleDescription(1, 0),
		    Usage = Usage.RenderTargetOutput,
		    BufferCount = 1,
		    ////OutputHandle = renderForm.Handle,
		    IsWindowed = true,
		};

		Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out var d3dDevice, out var swapChain);
		var d3dDeviceContext = d3dDevice.ImmediateContext;

		Texture2D backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
		var renderTargetView = new RenderTargetView(d3dDevice, backBuffer);

		// Draw
		d3dDeviceContext.OutputMerger.SetRenderTargets(renderTargetView);
		d3dDeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Mathematics.Interop.RawColor4(0.0f, 0.0f, 0.0f, 1.0f));
		swapChain.Present(1, PresentFlags.None);
	}
}
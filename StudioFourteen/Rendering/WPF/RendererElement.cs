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
using DependencyPropertyGenerator;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

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

public class WpfRenderer(D3D11Surface surface) : Renderer
{
	protected override Texture2D? GetBackBuffer()
	{
		if (surface.ActualWidth <= 0 || surface.ActualHeight <= 0)
			return null;

		if (!surface.IsVisible)
			return null;

		ModeDescription backBufferDesc = new ModeDescription((int)surface.ActualWidth, (int)surface.ActualHeight, new Rational(60, 1), Format.R8G8B8A8_UNorm);

		SwapChainDescription swapChainDesc = new SwapChainDescription()
		{
			ModeDescription = backBufferDesc,
			SampleDescription = new SampleDescription(1, 0),
			Usage = Usage.RenderTargetOutput,
			BufferCount = 2,
			IsWindowed = true,
		};

		Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, swapChainDesc, out var d3dDevice, out var swapChain);
		var d3dDeviceContext = d3dDevice.ImmediateContext;

		return swapChain.GetBackBuffer<Texture2D>(0);
	}

	protected override uint GetDeviceHeight() => (uint)surface.ActualHeight;
	protected override uint GetDeviceWidth() => (uint)surface.ActualWidth;
	protected override uint GetPendingDeviceHeight() => (uint)surface.ActualHeight;
	protected override uint GetPendingDeviceWidth() => (uint)surface.ActualWidth;
}

[DependencyProperty<double>("Sensitivity")]
[DependencyProperty<StudioFourteen.Transform>("Transform")]
public partial class TransformHandleControl : D3D11Surface
{
	private readonly WpfRenderer renderer;

	public TransformHandleControl()
	{
		this.renderer = new(this);
	}
}
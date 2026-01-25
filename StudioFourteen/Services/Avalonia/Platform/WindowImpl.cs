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

namespace StudioFourteen.Services.Avalonia.Platform;

using System;
using System.Collections.Generic;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Input.Raw;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;

public partial class WindowImpl : IWindowImpl
{
	public Window? Window;

	private readonly WindowRenderer windowRenderer;
	private readonly DxgiSurface glSurface;
	private readonly StudioScreens screen;
	private readonly Compositor compositor;

	public WindowImpl(Compositor compositor, StudioScreens screen)
	{
		this.glSurface = new DxgiSurface(this);
		this.screen = screen;
		this.windowRenderer = new(this, this.glSurface);
		this.compositor = compositor;

		this.Position = new PixelPoint(300, 100);
		this.ClientSize = new Size(256, 256);
	}

	public WindowState WindowState { get; set; }
	public Action<WindowState>? WindowStateChanged { get; set; }
	public Action? GotInputWhenDisabled { get; set; }
	public Func<WindowCloseReason, bool>? Closing { get; set; }

	public bool IsClientAreaExtendedToDecorations => false;
	public Action<bool>? ExtendClientAreaToDecorationsChanged { get; set; }
	public bool NeedsManagedDecorations => false;
	public Thickness ExtendedMargins => new Thickness(0);
	public Thickness OffScreenMargin => new Thickness(0);
	public WindowTransparencyLevel TransparencyLevel => WindowTransparencyLevel.Transparent;
	public AcrylicPlatformCompensationLevels AcrylicCompensationLevels { get; }

	public double DesktopScaling => 1;
	public double RenderScaling => 1;
	public Size MaxAutoSizeHint => new Size(4096, 4096);

	public Size? FrameSize { get; private set; }
	public Size ClientSize { get; private set; }

	public PixelPoint Position { get; private set; }
	public Action<PixelPoint>? PositionChanged { get; set; }
	public Action? Deactivated { get; set; }
	public Action? Activated { get; set; }
	public IPlatformHandle? Handle { get; }
	public Action<RawInputEventArgs>? Input { get; set; }
	public Action<Rect>? Paint { get; set; }
	public Action<Size, WindowResizeReason>? Resized { get; set; }
	public Action<double>? ScalingChanged { get; set; }
	public Action<WindowTransparencyLevel>? TransparencyLevelChanged { get; set; }
	public Action? Closed { get; set; }
	public Action? LostFocus { get; set; }
	public bool IsDisposed { get; private set; }

	public IEnumerable<object> Surfaces => [this.glSurface];
	public Compositor Compositor => this.compositor;

	public void Activate()
	{
	}

	public void BeginMoveDrag(PointerPressedEventArgs e)
	{
	}

	public void BeginResizeDrag(WindowEdge edge, PointerPressedEventArgs e)
	{
	}

	public void CanResize(bool value)
	{
	}

	public IPopupImpl? CreatePopup()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		this.IsDisposed = true;

		Studio.Avalonia.RenderPass.Remove(this.windowRenderer);
		this.windowRenderer.Dispose();
	}

	public void GetWindowsZOrder(Span<Window> windows, Span<long> zOrder)
	{
	}

	public void Show(bool activate, bool isDialog)
	{
		Studio.Avalonia.RenderPass.Add(this.windowRenderer);
	}

	public void Hide()
	{
		Studio.Avalonia.RenderPass.Remove(this.windowRenderer);
	}

	public Point PointToClient(PixelPoint point) => new Point(point.X - this.Position.X, point.Y - this.Position.Y);
	public PixelPoint PointToScreen(Point point) => this.Position + new PixelPoint((int)point.X, (int)point.Y);

	public void Move(PixelPoint point)
	{
		this.Position = point;
		this.PositionChanged?.Invoke(point);
	}

	public void Resize(Size clientSize, WindowResizeReason reason = WindowResizeReason.Application)
	{
		Size finalClientSize = new(
			Math.Clamp(clientSize.Width, 32, 4096),
			Math.Clamp(clientSize.Height, 32, 4096));

		this.ClientSize = finalClientSize;
		this.FrameSize = finalClientSize;

		this.Resized?.Invoke(finalClientSize, reason);
	}

	public void SetCanMaximize(bool value)
	{
	}

	public void SetCanMinimize(bool value)
	{
	}

	public void SetCursor(ICursorImpl? cursor)
	{
	}

	public void SetEnabled(bool enable)
	{
	}

	public void SetExtendClientAreaChromeHints(ExtendClientAreaChromeHints hints)
	{
	}

	public void SetExtendClientAreaTitleBarHeightHint(double titleBarHeight)
	{
	}

	public void SetExtendClientAreaToDecorationsHint(bool extendIntoClientAreaHint)
	{
	}

	public void SetFrameThemeVariant(PlatformThemeVariant themeVariant)
	{
	}

	public void SetIcon(IWindowIconImpl? icon)
	{
	}

	public void SetInputRoot(IInputRoot inputRoot)
	{
	}

	public void SetMinMaxSize(Size minSize, Size maxSize)
	{
	}

	public void SetParent(IWindowImpl? parent)
	{
	}

	public void SetSystemDecorations(SystemDecorations enabled)
	{
	}

	public void SetTitle(string? title)
	{
	}

	public void SetTopmost(bool value)
	{
	}

	public void SetTransparencyLevelHint(IReadOnlyList<WindowTransparencyLevel> transparencyLevels)
	{
	}

	public void ShowTaskbarIcon(bool value)
	{
	}

	public object? TryGetFeature(Type featureType)
	{
		if (featureType == typeof(IScreenImpl))
		{
			return this.screen;
		}

		/*if (featureType == typeof(ITextInputMethodImpl))
		{
			return Imm32InputMethod.Current;
		}

		if (featureType == typeof(INativeControlHostImpl))
		{
			return _nativeControlHost;
		}

		if (featureType == typeof(IStorageProvider))
		{
			return _storageProvider;
		}

		if (featureType == typeof(IClipboard))
		{
			return AvaloniaLocator.Current.GetRequiredService<IClipboard>();
		}

		if (featureType == typeof(IInputPane))
		{
			return _inputPane;
		}

		if (featureType == typeof(ILauncher))
		{
			return new BclLauncher();
		}*/

		return null;
	}

	public void HandleInput(RawInputEventArgs args)
	{
		this.Input?.Invoke(args);
	}
}

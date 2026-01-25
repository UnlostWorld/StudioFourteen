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

namespace StudioFourteen.Services.Avalonia;

using System;
using System.Numerics;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;
using global::Avalonia.Platform;

public partial class StudioWindowBase : Window
{
	public WindowReference? WindowReference;

	private PointerPoint? mouseDragStart;

	public StudioWindowBase()
	{
		this.InitializeComponent();
		this.Loaded += this.OnLoadComplete;
	}

	private void OnLoadComplete(object? sender, RoutedEventArgs e)
	{
		Screen? screen = this.Screens.ScreenFromWindow(this);
		if (screen == null)
			return;

		Size size = this.FrameSize ?? this.ClientSize;
		Vector2 pos = this.WindowReference?.DefaultPosition ?? new(0.5f, 0.5f);

		Vector2 screenPos = new(screen.WorkingArea.Width * pos.X, screen.WorkingArea.Height * pos.Y);
		Vector2 windowPos = new((float)size.Width * pos.X, (float)size.Height * pos.Y);

		this.Position = new PixelPoint(
			(int)(screenPos.X - windowPos.X),
			(int)(screenPos.Y - windowPos.Y));
	}

	private void OnPointerMoved(object? sender, PointerEventArgs e)
	{
		if (this.WindowReference?.CanDragMove == false)
			return;

		if (this.mouseDragStart == null)
			return;

		PointerPoint currentPoint = e.GetCurrentPoint(this);
		this.Position = new PixelPoint(
			this.Position.X + (int)(currentPoint.Position.X - this.mouseDragStart.Value.Position.X),
			this.Position.Y + (int)(currentPoint.Position.Y - this.mouseDragStart.Value.Position.Y));
	}

	private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		if (this.WindowReference?.CanDragMove == false)
			return;

		this.mouseDragStart = e.GetCurrentPoint(this);
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		if (this.WindowReference?.CanDragMove == false)
			return;

		this.mouseDragStart = null;
	}
}
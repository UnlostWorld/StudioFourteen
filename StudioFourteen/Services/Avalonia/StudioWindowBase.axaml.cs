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

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Input;

public partial class StudioWindowBase : Window
{
	public WindowReference? WindowReference;

	private PointerPoint? mouseDragStart;

	public StudioWindowBase()
	{
		this.InitializeComponent();
	}

	private void OnPointerMoved(object? sender, PointerEventArgs e)
	{
		if (this.mouseDragStart == null)
			return;

		PointerPoint currentPoint = e.GetCurrentPoint(this);
		this.Position = new PixelPoint(
			this.Position.X + (int)(currentPoint.Position.X - this.mouseDragStart.Value.Position.X),
			this.Position.Y + (int)(currentPoint.Position.Y - this.mouseDragStart.Value.Position.Y));
	}

	private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
	{
		this.mouseDragStart = e.GetCurrentPoint(this);
	}

	private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
	{
		this.mouseDragStart = null;
	}
}
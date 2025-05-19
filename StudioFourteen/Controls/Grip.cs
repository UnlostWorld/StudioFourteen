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

namespace StudioFourteen.Controls;

using System.Windows.Input;
using DependencyPropertyGenerator;
using StudioFourteen.Cursors;

[DependencyProperty<Directions>("Direction")]
public partial class Grip : StudioControl
{
	public Grip()
	{
		this.Cursor = this.Services.Cursor.GetCursor(CursorService.CursorType.Hand);
	}

	public enum Directions
	{
		Omni,
		Vertical,
		Horizontal,
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		this.Cursor = this.Services.Cursor.GetCursor(CursorService.CursorType.Grab);

		this.CaptureMouse();
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		this.Cursor = this.Services.Cursor.GetCursor(CursorService.CursorType.Hand);

		this.ReleaseMouseCapture();
	}
}
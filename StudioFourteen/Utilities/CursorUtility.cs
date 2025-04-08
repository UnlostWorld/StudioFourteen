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

namespace StudioFourteen.Utilities;

using System;
using System.Drawing;
using System.Numerics;
using Windows.Win32;

using DrawingPoint = System.Drawing.Point;
using Point = System.Windows.Point;

public static class CursorUtility
{
	private static bool cursorVisible = true;

	public static void SetCursorVisible(bool visible)
	{
		if (cursorVisible == visible)
			return;

		cursorVisible = visible;
		PInvoke.ShowCursor(visible);
	}

	public static Point GetPosition()
	{
		PInvoke.GetCursorPos(out DrawingPoint point);
		return point.ToPoint();
	}

	public static void SetPosition(Point position)
	{
		// Do not attempt to move the mouse cursor if its actually a tablet pen.
		if (ServiceManager.Instance.Tablet.PenPressure > 0)
			return;

		PInvoke.SetCursorPos((int)position.X, (int)position.Y);
	}

	public static IntPtr GetWindowUnderCursor()
	{
		PInvoke.GetCursorPos(out DrawingPoint point);
		return PInvoke.WindowFromPoint(point);
	}
}

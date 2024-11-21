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

using FFXIVClientStructs;
using System;
using System.Runtime.InteropServices;
using System.Windows;

public static class CursorUtility
{
	private static bool cursorVisible = true;

	public static void SetCursorVisible(bool visible)
	{
		if (cursorVisible == visible)
			return;

		cursorVisible = visible;
		ShowCursor(visible);
	}

	public static Point GetPosition()
	{
		Win32Point winPoint = default;
		GetCursorPos(ref winPoint);
		return new Point(winPoint.X, winPoint.Y);
	}

	public static void SetPosition(Point position)
	{
		SetCursorPos((int)position.X, (int)position.Y);
	}

	public static void SetPosition(Win32Point position)
	{
		SetCursorPos((int)position.X, (int)position.Y);
	}

	public static IntPtr GetWindowUnderCursor()
	{
		Win32Point winPoint = default;
		GetCursorPos(ref winPoint);
		return WindowFromPoint(winPoint);
	}

	public static Win32Point ToWin32Point(this Point point)
	{
		return new((uint)point.X, (uint)point.Y);
	}

	[DllImport("user32.dll")]
	private static extern int ShowCursor(bool bShow);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetCursorPos(ref Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetCursorPos(int x, int y);

	[DllImport("user32.dll")]
	private static extern IntPtr WindowFromPoint(Win32Point lpPoint);

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Point
	{
		public uint X;
		public uint Y;

		public Win32Point(uint x = 0, uint y = 0)
		{
			this.X = x;
			this.Y = y;
		}

		public Point ToPoint()
		{
			return new(this.X, this.Y);
		}
	}
}

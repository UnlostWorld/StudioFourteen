namespace StudioFourteen.Utilities;

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
	}
}

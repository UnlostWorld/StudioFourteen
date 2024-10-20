namespace StudioFourteen.Utilities;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

public static class XivWindow
{
	private static Rect size = default;

	public enum MouseKeys
	{
		Left = 0x01,
		Right = 0x02,
		Middle = 0x04,
		Four = 0x05,
		Five = 0x06,
	}

	public static Process? Process { get; set; }

	public static nint? Hwnd => Process?.MainWindowHandle;

	public static double TitleBarHeight => 22;

	public static Rect Size
	{
		get
		{
			if (Process == null)
				return new Rect(0, 0, 0, 0);

			GetWindowRect(Process.MainWindowHandle, out Win32Rect xivWindowRect);
			size.X = xivWindowRect.Left;
			size.Y = xivWindowRect.Top + TitleBarHeight;
			size.Width = xivWindowRect.Right - size.X;
			size.Height = xivWindowRect.Bottom - size.Y;
			return size;
		}
	}

	public static void Activate()
	{
		if (Process == null)
			return;

		SetForegroundWindow(Process.MainWindowHandle);
	}

	public static bool IsActive()
	{
		if (Process == null)
			return false;

		return GetForegroundWindow() == Process.MainWindowHandle;
	}

	public static void Embed(Window wnd)
	{
		if (Process == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		SetParent(wndInterop.Handle, Process.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(wndInterop.Handle, GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		SetWindowLong(wndInterop.Handle, GWL_STYLE, style);
	}

	public static void Unembed(Window wnd)
	{
		if (Process == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		SetParent(wndInterop.Handle, 0);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(wndInterop.Handle, GWL_STYLE);
		style = (int)((style & ~WS_CHILD) | WS_POPUP);
		SetWindowLong(wndInterop.Handle, GWL_STYLE, style);
	}

	public static void SetPosition(Window wnd, Point position)
	{
		if (Process == null)
			return;

		position.X = Math.Clamp(position.X, 0, 1);
		position.Y = Math.Clamp(position.X, 0, 1);

		WindowInteropHelper wndInterop = new(wnd);

		int x = (int)(XivWindow.Size.Width * position.X);
		int y = (int)(XivWindow.Size.Height * position.Y);

		if (wnd.SizeToContent == SizeToContent.Manual)
		{
			x = (int)((XivWindow.Size.Width * position.X) - (wnd.ActualWidth * position.X));
			y = (int)((XivWindow.Size.Height * position.Y) - (wnd.ActualHeight * position.Y));
		}

		int w = 0;
		int h = 0;

		// SHOWWINDOW | NOSIZE
		SetWindowPos(wndInterop.Handle, IntPtr.Zero, x, y, w, h, 0x0040 | 0x0001);

		// NOSIZE
		SetWindowPos(wndInterop.Handle, IntPtr.Zero, x, y, w, h, 0x0001);

		if (wnd.Topmost)
		{
			// NOSIZE | NOMOVE
			SetWindowPos(wndInterop.Handle, (IntPtr)(-1), x, y, w, h, 0x0001 | 0x0003);
		}
	}

	public static Point GetPosition(Window wnd)
	{
		Rect xivSize = XivWindow.Size;

		double l = (wnd.Left - xivSize.Left) / xivSize.Width;
		double t = (wnd.Top - xivSize.Top) / xivSize.Height;

		if (wnd.SizeToContent == SizeToContent.Manual)
		{
			l = (wnd.Left - xivSize.Left) / (xivSize.Width - wnd.ActualWidth);
			t = (wnd.Top - xivSize.Top) / (xivSize.Height - wnd.ActualHeight);
		}

		return new Point(l, t);
	}

	public static void SendKey(Key key, bool down)
	{
		if (Process == null)
			return;

		int virtualKey = KeyInterop.VirtualKeyFromKey(key);

		if (key == Key.LeftShift || key == Key.RightShift)
			virtualKey = 0x10;

		if (key == Key.LeftCtrl || key == Key.RightCtrl)
			virtualKey = 0x11;

		if (key == Key.LeftAlt || key == Key.RightAlt)
			virtualKey = 0x12;

		if (down)
		{
			PostMessage(Process.MainWindowHandle, 0x100, (IntPtr)virtualKey, IntPtr.Zero);
		}
		else
		{
			PostMessage(Process.MainWindowHandle, 0x0101, (IntPtr)virtualKey, IntPtr.Zero);
		}
	}

	public static Point? GetCursorPos()
	{
		if (XivWindow.Hwnd == null)
			return null;

		Win32Point p = default;
		GetCursorPos(ref p);

		// don't process mouse if its not over the xiv window
		IntPtr windowUnderCursor = WindowFromPoint(p);
		if (windowUnderCursor != XivWindow.Hwnd.Value)
			return null;

		ScreenToClient(XivWindow.Hwnd.Value, ref p);

		// don't process mouse if its outside the xiv window.
		Rect xivSize = XivWindow.Size;

		if (p.X < 0 || p.X > xivSize.Width)
			return null;

		if (p.Y < 0 || p.Y > xivSize.Height)
			return null;

		return new(p.X, p.Y);
	}

	public static bool GetMouseKey(MouseKeys key)
	{
		return (GetKeyState((int)key) & 0x8000) != 0;
	}

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetWindowRect(IntPtr hwnd, out Win32Rect rect);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetForegroundWindow(IntPtr hWnd);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr GetForegroundWindow();

	[DllImport("user32.dll")]
	private static extern IntPtr PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool GetCursorPos(ref Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ScreenToClient(IntPtr hWnd, ref Win32Point lpPoint);

	[DllImport("user32.dll")]
	private static extern IntPtr WindowFromPoint(Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetKeyState(int key);

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Point
	{
		public uint X;
		public uint Y;
	}
}

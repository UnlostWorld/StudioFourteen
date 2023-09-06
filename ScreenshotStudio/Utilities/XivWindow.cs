// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Utilities;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class XivWindow
{
	private static Rect size = new();

	public static Process? Process { get; set; }

	public static Rect Size
	{
		get
		{
			if (Process == null)
				return new Rect(0, 0, 0, 0);

			const double titlebarHeight = 22;

			GetWindowRect(Process.MainWindowHandle, out Win32Rect xivWindowRect);
			size.X = xivWindowRect.Left;
			size.Y = xivWindowRect.Top + titlebarHeight;
			size.Width = xivWindowRect.Right - size.X;
			size.Height = xivWindowRect.Bottom - size.Y;
			return size;
		}
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

		SetPosition(wnd, new Point(0.5, 0.5));
	}

	public static void SetPosition(Window wnd, Point position)
	{
		if (Process == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		int x = (int)((XivWindow.Size.Width * position.X) - (wnd.ActualWidth * position.X));
		int y = (int)((XivWindow.Size.Height * position.Y) - (wnd.ActualHeight * position.Y));
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

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}

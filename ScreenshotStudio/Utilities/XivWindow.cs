namespace ScreenshotStudio.Utilities;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class XivWindow
{
	private static Process? process;

	public static Process Process
	{
		get
		{
			if (process == null)
				process = Process.GetCurrentProcess();

			return process;
		}

		set => process = value;
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

		////SetPosition(wnd, new Point(0, 0));
	}

	public static void SetPosition(Window wnd, Point position)
	{
		if (Process == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		int x = (int)position.X;
		int y = (int)position.Y;
		int w = (int)wnd.ActualWidth;
		int h = (int)wnd.ActualHeight;

		/*int w = (int)wnd.ActualWidth - 1;
		int h = (int)wnd.ActualHeight - 1;

		int x = (int)(wnd.Left - position.Left);
		int y = (int)(wnd.Top - position.Top);

		x = Math.Clamp(x, 0, Math.Max((int)position.Width - w, 0));
		y = Math.Clamp(y, 0, Math.Max((int)position.Height - h, 0));*/

		// SHOWWINDOW
		SetWindowPos(wndInterop.Handle, IntPtr.Zero, x, y, w, h, 0x0040);

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
}

namespace ScreenshotStudio.Windows;

using ScreenshotStudio.Utilities;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public static class XivWindow
{
	public static void Embed(Window wnd)
	{
		if (XivProcessUtility.Process == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		SetParent(wndInterop.Handle, XivProcessUtility.Process.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(wndInterop.Handle, GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		SetWindowLong(wndInterop.Handle, GWL_STYLE, style);
	}

	/*protected override void UpdatePosition()
	{
		base.UpdatePosition();

		if (MemoryService.Process == null)
			return;

		Rect screenRect = this.ScreenRect;

		int w = (int)this.ActualWidth - 1;
		int h = (int)this.ActualHeight - 1;

		int x = (int)(this.Left - screenRect.Left);
		int y = (int)(this.Top - screenRect.Top);

		x = Math.Clamp(x, 0, Math.Max((int)screenRect.Width - w, 0));
		y = Math.Clamp(y, 0, Math.Max((int)screenRect.Height - h, 0));

		// SHOWWINDOW
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, x, y, w, h, 0x0040);

		// NOSIZE
		SetWindowPos(this.windowInteropHelper.Handle, IntPtr.Zero, x, y, w, h, 0x0001);

		if (this.Topmost)
		{
			// NOSIZE | NOMOVE
			SetWindowPos(this.windowInteropHelper.Handle, (IntPtr)(-1), x, y, w, h, 0x0001 | 0x0003);
		}
	}*/

	[DllImport("user32.dll", SetLastError = true)]
	private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

	[DllImport("user32.dll", EntryPoint = "GetWindowLong")]
	private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}

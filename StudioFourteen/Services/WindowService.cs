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

namespace StudioFourteen.Services;

using System;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using global::Windows.Win32;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;
using StudioFourteen.Services.Input.Devices;

public partial class WindowService : IService
{
	private readonly HWND windowHandle;
	private WndProcDelegate? wndProc;
	private nint oldWndProcPtr = 0;

	public WindowService()
	{
		Process process = Process.GetCurrentProcess();
		this.windowHandle = (HWND)process.MainWindowHandle;

		Studio.Tick.Add(Tick.TickChannels.LateGame, this.OnLateGameTick);
	}

	private delegate long WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

	public void Dispose()
	{
		if (this.oldWndProcPtr != 0)
		{
			nint wndProcPtr = PInvoke.SetWindowLongPtr(this.windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, this.oldWndProcPtr);
			Studio.Log.Information($"Restored wndProc from {wndProcPtr} to {this.oldWndProcPtr}");
			this.oldWndProcPtr = 0;
		}
	}

	public Vector2 ScreenToClient(Point p)
	{
		PInvoke.ScreenToClient(this.windowHandle, ref p);
		PInvoke.GetClientRect(this.windowHandle, out RECT rect);

		return new((float)p.X / (float)rect.Width, (float)p.Y / (float)rect.Height);
	}

	private void OnLateGameTick()
	{
		if (this.oldWndProcPtr != 0)
			return;

		// hook wndproc once we're in-game to ensure dalamud got to it first, otherwise stopping studio
		// will break dalamuds input. 😰
		uint mapId = Studio.ClientState.MapId;
		if (mapId > 0)
		{
			// https://github.com/ff-meli/ImGuiScene/blob/master/ImGuiScene/ImGui_Impl/Input/ImGui_Input_Impl_Direct.cs
			this.wndProc = this.WndProcDetour;
			nint wndProcPtr = Marshal.GetFunctionPointerForDelegate(this.wndProc);
			this.oldWndProcPtr = PInvoke.SetWindowLongPtr(this.windowHandle, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, wndProcPtr);
			Studio.Tick.Remove(Tick.TickChannels.LateGame, this.OnLateGameTick);
			Studio.Log.Information($"Replaced wndProc from {this.oldWndProcPtr} to {wndProcPtr}");
		}
	}

	private long WndProcDetour(nint hWnd, uint msg, ulong wParam, long lParam)
	{
		if (this.oldWndProcPtr == 0)
		{
			Studio.Log.Warning("No old wndproc defined!");
			return 0;
		}

		if (hWnd == this.windowHandle)
		{
			if (this.HandleWindowMessage(msg, wParam, lParam))
			{
				return 0;
			}
		}

		return CallWindowProc(this.oldWndProcPtr, hWnd, msg, wParam, lParam);
	}

	private bool HandleWindowMessage(uint msg, ulong wParam, long lParam)
	{
		if (Studio.IsDisposed || !Studio.IsInitialized)
			return false;

		WindowMessages message = (WindowMessages)msg;

		MouseDevice? mouseDevice = Studio.Input.Mouse;
		if (mouseDevice != null)
		{
			switch (message)
			{
				case WindowMessages.WM_MOUSEMOVE: return mouseDevice.ShouldConsumeMouse();
				case WindowMessages.WM_LBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButtons.Left, true);
				case WindowMessages.WM_LBUTTONUP: return mouseDevice.HandleMouseButton(MouseButtons.Left, false);
				case WindowMessages.WM_RBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButtons.Right, true);
				case WindowMessages.WM_RBUTTONUP: return mouseDevice.HandleMouseButton(MouseButtons.Right, false);
				case WindowMessages.WM_MBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButtons.Middle, true);
				case WindowMessages.WM_MBUTTONUP: return mouseDevice.HandleMouseButton(MouseButtons.Middle, false);

				case WindowMessages.WM_XBUTTONDOWN:
				{
					ulong hWord = (wParam >> 16) & 0xFFFF;
					return mouseDevice.HandleMouseButton((ushort)hWord == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2, true);
				}

				case WindowMessages.WM_XBUTTONUP:
				{
					ulong hWord = (wParam >> 16) & 0xFFFF;
					return mouseDevice.HandleMouseButton((ushort)hWord == 1 ? MouseButtons.XButton1 : MouseButtons.XButton2, false);
				}

				case WindowMessages.WM_MOUSEWHEEL:
				{
					ulong hWord = (wParam >> 16) & 0xFFFF;
					return mouseDevice.HandleMouseWheel((float)((short)hWord / 120.0f));
				}
			}
		}

		KeyboardDevice? keyboardDevice = Studio.Input.Keyboard;
		if (keyboardDevice != null)
		{
			switch (message)
			{
				// Keyboard
				case WindowMessages.WM_SYSKEYDOWN:
				case WindowMessages.WM_KEYDOWN: return keyboardDevice.HandleKey((int)wParam, true, lParam);
				case WindowMessages.WM_SYSKEYUP:
				case WindowMessages.WM_KEYUP: return keyboardDevice.HandleKey((int)wParam, false, lParam);
				case WindowMessages.WM_CHAR: return keyboardDevice.HandleChar((uint)wParam);
			}
		}

		return false;
	}

#pragma warning disable
	[DllImport("user32.dll")]
	private static extern long CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, ulong wParam, long lParam);

	public enum WindowMessages
	{
		WM_MOUSEMOVE = 0x0200,
		WM_LBUTTONDOWN = 0x0201,
		WM_LBUTTONUP = 0x0202,
		//// WM_LBUTTONDBLCLK = 0x0203,
		WM_RBUTTONDOWN = 0x0204,
		WM_RBUTTONUP = 0x0205,
		//// WM_RBUTTONDBLCLK = 0x0206,
		WM_MBUTTONDOWN = 0x0207,
		WM_MBUTTONUP = 0x0208,
		////WM_MBUTTONDBLCLK = 0x0209,
		WM_MOUSEWHEEL = 0x020A,
		WM_XBUTTONDOWN = 0x020B,
		WM_XBUTTONUP = 0x020C,
		////WM_XBUTTONDBLCLK = 0x020D,
		////WM_MOUSEHWHEEL = 0x020E,


		WM_KEYDOWN = 0x0100,
		WM_KEYUP = 0x0101,
		WM_CHAR = 0x0102,
		WM_SYSKEYDOWN = 0x0104,
		WM_SYSKEYUP = 0x0105,
	}
}
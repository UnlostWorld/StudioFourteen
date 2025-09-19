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

using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using FFXIVClientStructs.FFXIV.Component.GUI;
using StudioFourteen.Input;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using global::Windows.Win32;
using global::Windows.Win32.Foundation;
using global::Windows.Win32.UI.WindowsAndMessaging;

using DrawingPoint = System.Drawing.Point;
using Point = System.Windows.Point;

public partial class WindowService : ServiceBase
{
	private readonly Guid propertyGuid = Guid.NewGuid();
	private readonly HashSet<PanelWindow> mouseOverPanelWindows = new();
	private readonly Input0DListener clickActionListener;
	private readonly HashSet<IntPtr> studioWindowHwnds = new();
	private readonly HashSet<string> atkUnitBlacklist = new()
	{
		"GroupPoseStampImage",
		"CursorAddon",
		"_ActionDoubleCrossL",
		"_ActionDoubleCrossR",
		"_ActionContents",
		"_LimitBreak",
		"_ContentGauge",
	};

	private Rect xivClientSize;
	private unsafe AtkUnitBase* atkUnitUnderCursor;
	private PanelWindow? topMostPanelWindow;
	private PanelWindow? lastTopMostPanelWindow;
	private WndProcDelegate? wndProc;
	private nint oldWndProcPtr;

	public WindowService()
	{
		this.clickActionListener = new(InputAction.Focus_Game, "WindowService Focus Game");
		this.clickActionListener.Activate = this.OnFocusGame;
	}

	public delegate void OnXivClientSizeChanged(Rect newSize);
	private delegate long WndProcDelegate(IntPtr hWnd, uint msg, ulong wParam, long lParam);

	public event OnXivClientSizeChanged? XivClientSizeChanged;

	[Bind] public partial bool IsCursorOverAtkUnit { get; private set; }
	[Bind] public partial bool IsCursorOverImGui { get; private set; }
	[Bind] public partial bool IsCursorOverXiv { get; private set; }
	[Bind] public partial bool IsCursorOverStudio { get; private set; }

	public Process? XivProcess { get; set; }
	public nint? XivWindowHwnd => this.XivProcess?.MainWindowHandle;

	public override Task Initialize()
	{
		this.XivProcess = Process.GetCurrentProcess();

		if (!this.XivProcess.ProcessName.Contains("ffxiv"))
		{
			this.XivProcess = Process.GetProcessesByName("ffxiv_dx11").FirstOrDefault();
			this.Log.Warning($"Failed to get local XIV Process. This should never happen while running via Dalamud. Searching for process: {this.XivProcess}");
		}

		return base.Initialize();
	}

	public override void Attach()
	{
		base.Attach();
		this.clickActionListener.Enable();

  		// hook wndproc
		// https://github.com/ff-meli/ImGuiScene/blob/master/ImGuiScene/ImGui_Impl/Input/ImGui_Input_Impl_Direct.cs
		if (this.XivWindowHwnd != null)
	   	{
			this.wndProc = this.WndProcDetour;
			nint wndProcPtr = Marshal.GetFunctionPointerForDelegate(this.wndProc);
			this.oldWndProcPtr = PInvoke.SetWindowLongPtr((HWND)this.XivWindowHwnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, wndProcPtr);
	   	}

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Add(TickService.Channels.StudioTick, this.OnTick);
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.Services.Tick.Remove(TickService.Channels.StudioTick, this.OnTick);

		if (this.oldWndProcPtr != 0 && this.XivWindowHwnd != null)
		{
			PInvoke.SetWindowLongPtr((HWND)this.XivWindowHwnd, WINDOW_LONG_PTR_INDEX.GWL_WNDPROC, this.oldWndProcPtr);
			this.oldWndProcPtr = 0;
		}

		base.Detach();

		this.clickActionListener.Disable();
		this.Activate(null);
	}

	public Rect GetXivWindowClientSize()
	{
		if (this.XivProcess == null)
			return Rect.Empty;

		PInvoke.GetClientRect((HWND)this.XivProcess.MainWindowHandle, out RECT xivWindowRect);

		DrawingPoint tl = new(xivWindowRect.left, xivWindowRect.top);
		DrawingPoint br = new(xivWindowRect.right, xivWindowRect.bottom);
		PInvoke.ClientToScreen((HWND)this.XivProcess.MainWindowHandle, ref tl);
		PInvoke.ClientToScreen((HWND)this.XivProcess.MainWindowHandle, ref br);
		xivWindowRect.left = tl.X;
		xivWindowRect.top = tl.Y;
		xivWindowRect.right = br.X;
		xivWindowRect.bottom = br.Y;

		////PInvoke.GetWindowRect((HWND)this.XivProcess.MainWindowHandle, out RECT xivWindowRect);

		Rect size = default;
		size.X = xivWindowRect.left;
		size.Y = xivWindowRect.top;
		size.Width = xivWindowRect.right - size.X;
		size.Height = xivWindowRect.bottom - size.Y;
		return size;
	}

	public bool IsAnyWindowActive()
	{
		return PInvoke.GetForegroundWindow() != 0;
	}

	public bool IsAnyStudioWindowActive()
	{
		return this.topMostPanelWindow != null;
	}

	public bool IsActive(PanelWindow? window)
	{
		return this.topMostPanelWindow == window;
	}

	public bool WasLastActive(PanelWindow? window)
	{
		return this.lastTopMostPanelWindow == window;
	}

	public void Activate(PanelWindow? window)
	{
		this.lastTopMostPanelWindow = this.topMostPanelWindow;
		PanelWindow? newTopMost = window;
		this.topMostPanelWindow = window;

		if (this.lastTopMostPanelWindow != null && this.lastTopMostPanelWindow != window)
		{
			this.lastTopMostPanelWindow.Dispatcher.BeginInvoke(() =>
			{
				try
				{
					if (this.lastTopMostPanelWindow != null)
					{
						this.lastTopMostPanelWindow.IsForeground = false;
					}
				}
				catch (Exception)
				{
					// Sometimes we get threading issues here due to stale dispatchers, just ignore them.
				}
			});
		}

		if (newTopMost != null)
		{
			newTopMost.Dispatcher.BeginInvoke(() =>
			{
				WindowInteropHelper wndInterop = new(newTopMost);
				PInvoke.BringWindowToTop((HWND)wndInterop.Handle);
				PInvoke.SetFocus((HWND)wndInterop.Handle);

				newTopMost.IsForeground = true;
			});
		}
		else
		{
			if (this.XivWindowHwnd != null)
			{
				PInvoke.SetForegroundWindow((HWND)this.XivWindowHwnd);
				PInvoke.SetFocus((HWND)this.XivWindowHwnd);
			}
		}
	}

	public void SetMouseOver(PanelWindow window, bool enter)
	{
		if (enter)
		{
			this.mouseOverPanelWindows.Add(window);
		}
		else
		{
			this.mouseOverPanelWindows.Remove(window);
		}
	}

	public bool IsMouseOverWindow()
	{
		return this.mouseOverPanelWindows.Count > 0;
	}

	public PanelWindow? GetMouseOverWindow()
	{
		return this.mouseOverPanelWindows.FirstOrDefault();
	}

	public void SendToBack(Window window)
	{
		window.Dispatcher.Invoke(() =>
		{
			WindowInteropHelper wndInterop = new(window);

			SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)new IntPtr(1), 0, 0, 0, 0, flags);
		});
	}

	public void Embed(Window wnd)
	{
		if (this.XivProcess == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		wnd.ShowInTaskbar = false;

		if (!DalamudServices.IsWine)
		{
			PInvoke.SetWindowLong(
				(HWND)wndInterop.Handle,
				WINDOW_LONG_PTR_INDEX.GWL_STYLE,
				(int)WINDOW_STYLE.WS_CHILD);

			PInvoke.SetWindowLong(
				(HWND)wndInterop.Handle,
				WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE,
				(int)WINDOW_EX_STYLE.WS_EX_TOOLWINDOW);

			PInvoke.SetParent((HWND)wndInterop.Handle, (HWND)this.XivProcess.MainWindowHandle);
		}
		else
		{
			wnd.Topmost = true;
		}

		System.Windows.Point p = this.GetPosition(wnd);
		this.SetPosition(wnd, p, true);
	}

	public void Unembed(Window wnd)
	{
		if (this.XivProcess == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		wnd.ShowInTaskbar = true;

		if (!DalamudServices.IsWine)
		{
			PInvoke.SetParent((HWND)wndInterop.Handle, (HWND)0u);

			PInvoke.SetWindowLong(
				(HWND)wndInterop.Handle,
				WINDOW_LONG_PTR_INDEX.GWL_STYLE,
				unchecked((int)WINDOW_STYLE.WS_POPUP));
		}
		else
		{
			wnd.Topmost = false;
		}
	}

	public void SetPosition(Window wnd, System.Windows.Point position, bool setZ)
	{
		if (this.XivProcess == null)
			return;

		Rect xivWindowSize = this.GetXivWindowClientSize();

		position.X = Math.Clamp(position.X, 0, 1);
		position.Y = Math.Clamp(position.Y, 0, 1);

		WindowInteropHelper wndInterop = new(wnd);

		int x = (int)(xivWindowSize.Width * position.X);
		int y = (int)(xivWindowSize.Height * position.Y);

		if (wnd.SizeToContent == SizeToContent.Manual)
		{
			x = (int)((xivWindowSize.Width * position.X) - (wnd.ActualWidth * position.X));
			y = (int)((xivWindowSize.Height * position.Y) - (wnd.ActualHeight * position.Y));
		}

		// TODO: || !wnd.IsEmbedded
		if (DalamudServices.IsWine)
		{
			x += (int)xivWindowSize.Left;
			y += (int)xivWindowSize.Top;
		}

		int w = 0;
		int h = 0;

		if (setZ)
		{
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
		}
		else
		{
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
		}

		if (wnd.Topmost)
		{
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)(IntPtr)(-1), x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
		}
	}

	public System.Windows.Point GetPosition(Window wnd)
	{
		Rect xivSize = this.GetXivWindowClientSize();

		double al = 0;
		double at = 0;

		// TODO: || !wnd.IsEmbedded
		if (DalamudServices.IsWine)
		{
			al = (int)xivSize.Left;
			at = (int)xivSize.Top;
		}

		double l = (wnd.Left - xivSize.Left - al) / xivSize.Width;
		double t = (wnd.Top - xivSize.Top - at) / xivSize.Height;

		if (wnd.SizeToContent == SizeToContent.Manual)
		{
			l = (wnd.Left - xivSize.Left) / (xivSize.Width - wnd.ActualWidth);
			t = (wnd.Top - xivSize.Top) / (xivSize.Height - wnd.ActualHeight);
		}

		return new System.Windows.Point(l, t);
	}

	public void SendKeyToXiv(VirtualKey key, bool down)
	{
		if (this.XivProcess == null)
			return;

		uint wmKeyDown = 0x0100;
		uint wmKeyUp = 0x0101;

		if (down)
		{
			PInvoke.PostMessage((HWND)this.XivProcess.MainWindowHandle, wmKeyDown, new WPARAM((nuint)key), IntPtr.Zero);
		}
		else
		{
			PInvoke.PostMessage((HWND)this.XivProcess.MainWindowHandle, wmKeyUp, new WPARAM((nuint)key), IntPtr.Zero);
		}
	}

	public void SetCursorPosition(Point position)
	{
		if (this.XivWindowHwnd == null)
			return;

		DrawingPoint p = position.ToDrawingPoint();
		PInvoke.ClientToScreen((HWND)this.XivWindowHwnd.Value, ref p);
		CursorUtility.SetPosition(p.ToPoint());
	}

	public void SetCursorPosition(Vector2 position)
	{
		if (this.XivWindowHwnd == null)
			return;

		Rect xivSize = this.GetXivWindowClientSize();

		position.X *= (float)xivSize.Width;
		position.Y *= (float)xivSize.Height;

		DrawingPoint p = new((int)position.X, (int)position.Y);
		PInvoke.ClientToScreen((HWND)this.XivWindowHwnd.Value, ref p);
		CursorUtility.SetPosition(p.ToPoint());
	}

	public Point? GetCursorPosition()
	{
		if (this.XivWindowHwnd == null)
			return null;

		DrawingPoint position = CursorUtility.GetPosition().ToDrawingPoint();

		PInvoke.ScreenToClient((HWND)this.XivWindowHwnd.Value, ref position);

		return new(position.X, position.Y);
	}

	public void OnWindowOpening(Window window)
	{
		lock(this.studioWindowHwnds)
		{
			WindowInteropHelper windowInteropHelper = new(window);
			this.studioWindowHwnds.Add(windowInteropHelper.Handle);
		}
	}

	public void OnWindowClosing(Window window)
	{
		lock(this.studioWindowHwnds)
		{
			WindowInteropHelper windowInteropHelper = new(window);
			this.studioWindowHwnds.Remove(windowInteropHelper.Handle);
		}
	}

	protected unsafe void OnGameTick()
	{
		AtkUnitBase* pAtkUnit = this.GetAtkUnitUnderCursor();

		this.atkUnitUnderCursor = pAtkUnit;
		this.IsCursorOverAtkUnit = pAtkUnit != null;
		this.IsCursorOverImGui = this.GetIsCursorOverImGui();
	}

	protected unsafe void OnTick()
	{
		this.IsCursorOverXiv = this.GetIsCursorOverXiv();
		this.IsCursorOverStudio = this.GetIsCursorOverStudio();

		Rect xivClientRect = this.GetXivWindowClientSize();
		if (xivClientRect != this.xivClientSize)
		{
			this.xivClientSize = xivClientRect;
			this.XivClientSizeChanged?.Invoke(this.xivClientSize);
		}
	}

	private void OnFocusGame()
	{
		this.Activate(null);
	}

	private bool GetIsCursorOverXiv()
	{
		return CursorUtility.GetWindowUnderCursor() == this.XivWindowHwnd;
	}

	private bool GetIsCursorOverStudio()
	{
		lock(this.studioWindowHwnds)
		{
			IntPtr hwnd = CursorUtility.GetWindowUnderCursor();
			return this.studioWindowHwnds.Contains(hwnd);
		}
	}

	private bool GetIsCursorOverImGui()
	{
		return ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow) || ImGui.IsAnyItemHovered();
	}

	private unsafe AtkUnitBase* GetAtkUnitUnderCursor()
	{
		Point? cursorPos = this.GetCursorPosition();
		if (cursorPos == null)
			return null;

		AtkUnitList? loadedUnits = AtkManager.GetAllLoadedUnits();
		if (loadedUnits == null)
			return null;

		AtkStage* atkStage = AtkStage.Instance();
		if (atkStage == null)
			return null;

		for (int i = 0; i < loadedUnits.Value.Count; i++)
		{
			AtkUnitBase* unit = loadedUnits.Value.Entries[i];

			if (!unit->IsVisible)
				continue;

			// HACK: yes/no box is always considered under the mouse, while changing resolution
			// the X/Y pos of the box is wrong, so we can't test it.
			if (unit->NameString == "SelectYesno")
				return unit;

			if (unit->Alpha < 32)
				continue;

			if (unit->DepthLayer < 4)
				continue;

			if (unit->VisibilityFlags != 0)
				continue;

			if (unit->WindowCollisionNode == null)
				continue;

			if (unit->WindowCollisionNode->Alpha_2 < 32)
				continue;

			if (this.atkUnitBlacklist.Contains(unit->NameString))
				continue;

			Rect windowBounds = new(unit->X, unit->Y, unit->GetScaledWidth(true), unit->GetScaledHeight(true));

			if (cursorPos.Value.X > windowBounds.Left
				&& cursorPos.Value.X < windowBounds.Right
				&& cursorPos.Value.Y > windowBounds.Top
				&& cursorPos.Value.Y < windowBounds.Bottom)
			{
				////this.Log.Information($">> {unit->NameString}");
				return unit;
			}
		}

		return null;
	}

	private long WndProcDetour(nint hWnd, uint msg, ulong wParam, long lParam)
	{
		if (this.oldWndProcPtr == 0)
		{
			this.Log.Warning("No old wndproc defined!");
			return 0;
		}

		if (hWnd == this.XivWindowHwnd)
		{
			if (this.HandleWindowMessage(msg, wParam))
			{
				return 0;
			}
		}

		return CallWindowProc(this.oldWndProcPtr, hWnd, msg, wParam, lParam);
	}

	private bool HandleWindowMessage(uint msg, ulong wParam)
	{
		WindowMessages message = (WindowMessages)msg;

		Input.Devices.MouseDevice? mouseDevice = this.Services.Input.Mouse;
		if (mouseDevice == null)
			return false;

		Input.Devices.KeyboardDevice? keyboardDevice = this.Services.Input.Keyboard;
		if (keyboardDevice == null)
			return false;

		ulong hWord = (wParam >> 16) & 0xFFFF;
		switch (message)
		{
			// Mouse
			case WindowMessages.WM_LBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButton.Left, true);
			case WindowMessages.WM_LBUTTONUP: return mouseDevice.HandleMouseButton(MouseButton.Left, false);
			case WindowMessages.WM_RBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButton.Right, true);
			case WindowMessages.WM_RBUTTONUP: return mouseDevice.HandleMouseButton(MouseButton.Right, false);
			case WindowMessages.WM_MBUTTONDOWN: return mouseDevice.HandleMouseButton(MouseButton.Middle, true);
			case WindowMessages.WM_MBUTTONUP: return mouseDevice.HandleMouseButton(MouseButton.Middle, false);
			case WindowMessages.WM_XBUTTONDOWN: return mouseDevice.HandleMouseButton((ushort)hWord == 1 ? MouseButton.XButton1 : MouseButton.XButton2, true);
			case WindowMessages.WM_XBUTTONUP: return mouseDevice.HandleMouseButton((ushort)hWord == 1 ? MouseButton.XButton1 : MouseButton.XButton2, false);
			case WindowMessages.WM_MOUSEWHEEL: return mouseDevice.HandleMouseWheel((float)((short)hWord / 120.0f));

			// Keyboard
			case WindowMessages.WM_SYSKEYDOWN:
			case WindowMessages.WM_KEYDOWN: return keyboardDevice.HandleKey((int)wParam, true);
			case WindowMessages.WM_SYSKEYUP:
			case WindowMessages.WM_KEYUP: return keyboardDevice.HandleKey((int)wParam, false);
			case WindowMessages.WM_CHAR: return keyboardDevice.HandleChar((uint)wParam);
		}

		return false;
	}

#pragma warning disable
	[DllImport("user32.dll")]
    private static extern long CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint Msg, ulong wParam, long lParam);

	public enum WindowMessages
	{
        ////WM_MOUSEMOVE = 0x0200,
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

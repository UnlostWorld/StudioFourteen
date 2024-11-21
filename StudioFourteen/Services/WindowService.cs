// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Services;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using Setter = PropertyChanged.SourceGenerator.Setter;

public partial class WindowService : ServiceBase
{
	private readonly HashSet<IntPtr> studioWindowHwnds = new();
	private readonly HashSet<string> atkUnitBlacklist = new()
	{
		"GroupPoseStampImage",
		"CursorAddon",
	};

	[Notify(Setter.Private)] private bool isCursorOverAtkUnit;
	[Notify(Setter.Private)] private bool isCursorOverImGui;
	[Notify(Setter.Private)] private bool isCursorOverXiv;
	[Notify(Setter.Private)] private bool isCursorOverStudio;

	[Notify(Setter.Private)] private bool enableXivWindowOverlay;

	private unsafe AtkUnitBase* atkUnitUnderCursor;

	public Process? XivProcess { get; set; }
	public nint? XivWindowHwnd => this.XivProcess?.MainWindowHandle;
	public double TitleBarHeight => 18;

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

	public Rect GetXivWindowSize()
	{
		if (this.XivProcess == null)
			return Rect.Empty;

		GetWindowRect(this.XivProcess.MainWindowHandle, out Win32Rect xivWindowRect);

		Rect size = default;
		size.X = xivWindowRect.Left;
		size.Y = xivWindowRect.Top + this.TitleBarHeight;
		size.Width = xivWindowRect.Right - size.X;
		size.Height = xivWindowRect.Bottom - size.Y;
		return size;
	}

	public bool IsAnyStudioWindowActive()
	{
		return this.studioWindowHwnds.Contains(GetForegroundWindow());
	}

	public void ActivateXivWindow()
	{
		if (this.XivWindowHwnd == null)
			return;

		SetForegroundWindow((IntPtr)this.XivWindowHwnd);
	}

	public bool IsXivWindowActive()
	{
		if (this.XivWindowHwnd == null)
			return false;

		return GetForegroundWindow() == this.XivWindowHwnd;
	}

	public void BringToTop(Window window)
	{
		this.BringXivWindowToTop();

		WindowInteropHelper wndInterop = new(window);
		BringWindowToTop(wndInterop.Handle);
	}

	public void SendToBack(Window window)
	{
		WindowInteropHelper wndInterop = new(window);
		SetWindowPos(wndInterop.Handle, new IntPtr(1), 0, 0, 0, 0, 0x0001 | 0x0002 | 0x0010);
	}

	public void BringXivWindowToTop()
	{
		if (this.XivWindowHwnd == null)
			return;

		BringWindowToTop((IntPtr)this.XivWindowHwnd);
	}

	public void Embed(Window wnd)
	{
		if (this.XivProcess == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		SetParent(wndInterop.Handle, this.XivProcess.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;
		const int GWL_STYLE = -16;

		int style = GetWindowLong(wndInterop.Handle, GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		SetWindowLong(wndInterop.Handle, GWL_STYLE, style);
	}

	public void Unembed(Window wnd)
	{
		if (this.XivProcess == null)
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

	public void SetPosition(Window wnd, Point position)
	{
		if (this.XivProcess == null)
			return;

		Rect xivWindowSize = this.GetXivWindowSize();

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

	public Point GetPosition(Window wnd)
	{
		Rect xivSize = this.GetXivWindowSize();

		double l = (wnd.Left - xivSize.Left) / xivSize.Width;
		double t = (wnd.Top - xivSize.Top) / xivSize.Height;

		if (wnd.SizeToContent == SizeToContent.Manual)
		{
			l = (wnd.Left - xivSize.Left) / (xivSize.Width - wnd.ActualWidth);
			t = (wnd.Top - xivSize.Top) / (xivSize.Height - wnd.ActualHeight);
		}

		return new Point(l, t);
	}

	public void SendKey(Key key, bool down)
	{
		if (this.XivProcess == null)
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
			PostMessage(this.XivProcess.MainWindowHandle, 0x100, (IntPtr)virtualKey, IntPtr.Zero);
		}
		else
		{
			PostMessage(this.XivProcess.MainWindowHandle, 0x0101, (IntPtr)virtualKey, IntPtr.Zero);
		}
	}

	public void SetCursorPosition(Point pos)
	{
		if (this.XivWindowHwnd == null)
			return;

		CursorUtility.Win32Point p = default;
		p.X = (uint)pos.X;
		p.Y = (uint)pos.Y;
		ClientToScreen(this.XivWindowHwnd.Value, ref p);

		CursorUtility.SetPosition(p);
	}

	public Point? GetCursorPosition()
	{
		if (this.XivWindowHwnd == null)
			return null;

		Point position = CursorUtility.GetPosition();
		CursorUtility.Win32Point p = new((uint)position.X, (uint)position.Y);

		ScreenToClient(this.XivWindowHwnd.Value, ref p);

		// don't process mouse if its outside the xiv window.
		Rect xivSize = this.GetXivWindowSize();

		if (p.X < 0 || p.X > xivSize.Width)
			return null;

		if (p.Y < 0 || p.Y > xivSize.Height)
			return null;

		return new(p.X, p.Y);
	}

	public void OnWindowOpening(Window window)
	{
		WindowInteropHelper windowInteropHelper = new(window);
		this.studioWindowHwnds.Add(windowInteropHelper.Handle);
	}

	public void OnWindowClosing(Window window)
	{
		WindowInteropHelper windowInteropHelper = new(window);
		this.studioWindowHwnds.Remove(windowInteropHelper.Handle);
	}

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		AtkUnitBase* pAtkUnit = this.GetAtkUnitUnderCursor();
		this.atkUnitUnderCursor = pAtkUnit;
		this.IsCursorOverAtkUnit = pAtkUnit != null;
		this.IsCursorOverImGui = this.GetIsCursorOverImGui();
		this.IsCursorOverXiv = this.GetIsCursorOverXiv();
		this.IsCursorOverStudio = this.GetIsCursorOverStudio();

		this.EnableXivWindowOverlay = (!this.IsCursorOverXiv && !this.IsCursorOverStudio) || (!this.IsCursorOverAtkUnit && !this.IsCursorOverImGui);
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
	private static extern bool ScreenToClient(IntPtr hWnd, ref CursorUtility.Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool ClientToScreen(IntPtr hWnd, ref CursorUtility.Win32Point lpPoint);

	[DllImport("user32.dll", SetLastError = true)]
	private static extern bool BringWindowToTop(IntPtr hWnd);

	private bool GetIsCursorOverXiv()
	{
		return CursorUtility.GetWindowUnderCursor() == this.XivWindowHwnd;
	}

	private bool GetIsCursorOverStudio()
	{
		IntPtr hwnd = CursorUtility.GetWindowUnderCursor();
		return this.studioWindowHwnds.Contains(hwnd);
	}

	private bool GetIsCursorOverImGui()
	{
		return ImGui.IsWindowHovered(ImGuiHoveredFlags.AnyWindow | ImGuiHoveredFlags.AllowWhenOverlapped | ImGuiHoveredFlags.AllowWhenDisabled)
			|| ImGui.IsAnyItemHovered();
	}

	private unsafe AtkUnitBase* GetAtkUnitUnderCursor()
	{
		Point? cursorPos = this.GetCursorPosition();
		if (cursorPos == null)
			return null;

		AtkUnitList? loadedUnits = AtkManager.GetAllLoadedUnits();
		if (loadedUnits == null)
			return null;

		for (int i = 0; i < loadedUnits.Value.Count; i++)
		{
			AtkUnitBase* unit = loadedUnits.Value.Entries[i];

			if (!unit->IsVisible)
				continue;

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
				return unit;
			}
		}

		return null;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct Win32Rect
	{
		public int Left;        // x position of upper-left corner
		public int Top;         // y position of upper-left corner
		public int Right;       // x position of lower-right corner
		public int Bottom;      // y position of lower-right corner
	}
}

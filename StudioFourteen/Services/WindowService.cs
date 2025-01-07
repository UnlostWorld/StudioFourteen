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

using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Studio;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;
using static FFXIVClientStructs.FFXIV.Client.Game.InstanceContent.PublicContentBozja.Delegates;
using Point = System.Drawing.Point;
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

	private Rect xivClientSize;

	private unsafe AtkUnitBase* atkUnitUnderCursor;

	public delegate void OnXivClientSizeChanged(Rect newSize);

	public event OnXivClientSizeChanged? XivClientSizeChanged;

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

	public Rect GetXivWindowClientSize()
	{
		if (this.XivProcess == null)
			return Rect.Empty;

		PInvoke.GetClientRect((HWND)this.XivProcess.MainWindowHandle, out RECT xivWindowRect);

		Point tl = new(xivWindowRect.left, xivWindowRect.top);
		Point br = new(xivWindowRect.right, xivWindowRect.bottom);
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
		return Windows.Win32.PInvoke.GetForegroundWindow() != 0;
	}

	public bool IsAnyStudioWindowActive()
	{
		return this.studioWindowHwnds.Contains(PInvoke.GetForegroundWindow());
	}

	public void ActivateStudioWindow()
	{
		if (BackgroundWindow.Instance == null)
			return;

		BackgroundWindow.Instance.Dispatcher.Invoke(() =>
		{
			BackgroundWindow.Instance.Activate();
		});
	}

	public void ActivateXivWindow()
	{
		if (this.XivWindowHwnd == null)
			return;

		PInvoke.SetForegroundWindow((HWND)this.XivWindowHwnd);
	}

	public bool IsXivWindowActive()
	{
		if (this.XivWindowHwnd == null)
			return false;

		return PInvoke.GetForegroundWindow() == this.XivWindowHwnd;
	}

	public void BringToTop(Window window)
	{
		this.BringXivWindowToTop();

		WindowInteropHelper wndInterop = new(window);
		PInvoke.BringWindowToTop((HWND)wndInterop.Handle);
	}

	public void SendToBack(Window window)
	{
		WindowInteropHelper wndInterop = new(window);

		SET_WINDOW_POS_FLAGS flags = SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE;
		PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)new IntPtr(1), 0, 0, 0, 0, flags);
	}

	public void BringXivWindowToTop()
	{
		if (this.XivWindowHwnd == null)
			return;

		PInvoke.BringWindowToTop((HWND)this.XivWindowHwnd);
	}

	public void Embed(Window wnd)
	{
		if (this.XivProcess == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		PInvoke.SetParent((HWND)wndInterop.Handle, (HWND)this.XivProcess.MainWindowHandle);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;

		int style = PInvoke.GetWindowLong((HWND)wndInterop.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		style = (int)((style & ~WS_POPUP) | WS_CHILD);
		PInvoke.SetWindowLong((HWND)wndInterop.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

		// TODO: This GetPosition only works for embedded windows, we should add a function to do the ClintRect
		// conversion to find its relative position even while not embedded.
		System.Windows.Point p = this.GetPosition(wnd);
		this.SetPosition(wnd, p);
	}

	public void Unembed(Window wnd)
	{
		if (this.XivProcess == null)
			return;

		WindowInteropHelper wndInterop = new(wnd);

		PInvoke.SetParent((HWND)wndInterop.Handle, (HWND)0);

		const uint WS_POPUP = 0x80000000;
		const uint WS_CHILD = 0x40000000;

		int style = PInvoke.GetWindowLong((HWND)wndInterop.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
		style = (int)((style & ~WS_CHILD) | WS_POPUP);
		PInvoke.SetWindowLong((HWND)wndInterop.Handle, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);
	}

	public void SetPosition(Window wnd, System.Windows.Point position)
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

		int w = 0;
		int h = 0;

		PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_SHOWWINDOW | SET_WINDOW_POS_FLAGS.SWP_NOSIZE);
		PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)IntPtr.Zero, x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_NOSIZE);

		if (wnd.Topmost)
		{
			PInvoke.SetWindowPos((HWND)wndInterop.Handle, (HWND)(IntPtr)(-1), x, y, w, h, SET_WINDOW_POS_FLAGS.SWP_NOSIZE | SET_WINDOW_POS_FLAGS.SWP_NOMOVE);
		}
	}

	public System.Windows.Point GetPosition(Window wnd)
	{
		Rect xivSize = this.GetXivWindowClientSize();

		double l = (wnd.Left - xivSize.Left) / xivSize.Width;
		double t = (wnd.Top - xivSize.Top) / xivSize.Height;

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

	public void SetCursorPosition(Point pos)
	{
		if (this.XivWindowHwnd == null)
			return;

		PInvoke.ClientToScreen((HWND)this.XivWindowHwnd.Value, ref pos);
		CursorUtility.SetPosition(pos);
	}

	public Point? GetCursorPosition()
	{
		if (this.XivWindowHwnd == null)
			return null;

		Point position = CursorUtility.GetPosition();

		PInvoke.ScreenToClient((HWND)this.XivWindowHwnd.Value, ref position);

		// don't process mouse if its outside the xiv window.
		Rect xivSize = this.GetXivWindowClientSize();

		if (position.X < 0 || position.X > xivSize.Width)
			return null;

		if (position.Y < 0 || position.Y > xivSize.Height)
			return null;

		return new(position.X, position.Y);
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

		////if (pAtkUnit != null)
		////	this.Log.Information($">> {pAtkUnit->NameString}");

		this.atkUnitUnderCursor = pAtkUnit;
		this.IsCursorOverAtkUnit = pAtkUnit != null;
		this.IsCursorOverImGui = this.GetIsCursorOverImGui();
		this.IsCursorOverXiv = this.GetIsCursorOverXiv();
		this.IsCursorOverStudio = this.GetIsCursorOverStudio();

		this.EnableXivWindowOverlay = ((!this.IsCursorOverXiv && !this.IsCursorOverStudio) || (!this.IsCursorOverAtkUnit && !this.IsCursorOverImGui))
			&& !this.Services.Reshade.IsReshadeOverlayOpen;

		Rect xivClientRect = this.GetXivWindowClientSize();
		if (xivClientRect != this.xivClientSize)
		{
			this.xivClientSize = xivClientRect;
			this.XivClientSizeChanged?.Invoke(this.xivClientSize);
		}
	}

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
}

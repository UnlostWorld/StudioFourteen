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

namespace StudioFourteen.Cursors;

using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using StudioFourteen.Controls;
using System.Windows.Input;
using FFXIVClientStructs.FFXIV.Client.UI;
using StudioFourteen.Interop;
using StudioFourteen.Launcher;
using StudioFourteen.Panels;
using StudioFourteen.Services;
using Windows.Win32.UI.WindowsAndMessaging;
using WpfUtils.Controls;

public class CursorService : ServiceBase
{
	private readonly Cursor xivPointer;
	private readonly Cursor xivLink;
	private readonly Cursor xivGrab;
	private readonly Cursor xivHand;

	public CursorService()
	{
		this.xivPointer = this.LoadCursor("pointer.cur");
		this.xivLink = this.LoadCursor("link.cur");
		this.xivGrab = this.LoadCursor("grab.cur");
		this.xivHand = this.LoadCursor("hand.cur");

		this.SetCursor<PanelWindow>(CursorType.Pointer);
		this.SetCursor<PopOut>(CursorType.Pointer);
		this.SetCursor<LauncherMenu>(CursorType.Pointer);
		this.SetCursor<Window>(CursorType.Pointer);
		this.SetCursor<ToggleButton>(CursorType.Link);
		this.SetCursor<ListBoxItem>(CursorType.Link);
		this.SetCursor<Button>(CursorType.Link);
		this.SetCursor<Grip>(CursorType.Hand);
	}

	public enum CursorType
	{
		Pointer,
		Link,
		Hand,
		Grab,
	}

	public unsafe override void Attach()
	{
		base.Attach();

		Hooks.UpdateGameCursor.Enable(this.UpdateCursorDetour);
		Hooks.SetCursor.Enable(this.SetCursorDetour);
	}

	public override void Detach()
	{
		base.Detach();
		Hooks.UpdateGameCursor.Disable();
		Hooks.SetCursor.Disable();
	}

	public void SetCursor(CursorType type)
	{
		Mouse.SetCursor(this.GetCursor(type));
	}

	public Cursor GetCursor(CursorType type)
	{
		bool useSystem = this.Settings.UseSystemCursors;
		switch (type)
		{
			case CursorType.Pointer: return useSystem ? Cursors.Arrow : this.xivPointer;
			case CursorType.Link: return useSystem ? Cursors.Hand : this.xivLink;
			case CursorType.Grab: return useSystem ? Cursors.Hand : this.xivGrab;
			case CursorType.Hand: return useSystem ? Cursors.Arrow : this.xivHand;
		}

		throw new NotSupportedException();
	}

	private unsafe nint UpdateCursorDetour(RaptureAtkModule* module)
    {
		return Hooks.UpdateGameCursor.Original(module);
    }

	private IntPtr SetCursorDetour(HCURSOR hCursor)
	{
		return IntPtr.Zero;
	}

	private Cursor LoadCursor(string name)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Cursors.{name}";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);
		if (stream == null)
			throw new Exception($"Cursor \"{name}\" not found in manifest resources");

		return new Cursor(stream);
	}

	private void SetCursor<T>(CursorType type)
		where T : FrameworkElement
	{
		EventManager.RegisterClassHandler(
			typeof(T),
			FrameworkElement.MouseEnterEvent,
			new RoutedEventHandler((s, e) =>
			{
				if (ServiceManager.ShutdownRequested)
					return;

				if (s is FrameworkElement fe)
				{
					fe.Cursor = ServiceManager.Instance.Cursor.GetCursor(type);
				}
			}));
	}
}
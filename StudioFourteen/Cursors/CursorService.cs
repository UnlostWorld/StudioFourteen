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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
	public CursorService()
	{
		this.Pointer = this.LoadCursor("pointer.cur");
		this.Link = this.LoadCursor("link.cur");
		this.Grab = this.LoadCursor("grab.cur");

		this.SetCursor<PanelWindow>(this.Pointer);
		this.SetCursor<PopOut>(this.Pointer);
		this.SetCursor<LauncherMenu>(this.Pointer);
		this.SetCursor<Window>(this.Pointer);
		this.SetCursor<ToggleButton>(this.Link);
		this.SetCursor<ListBoxItem>(this.Link);
		this.SetCursor<Button>(this.Link);
	}

	public Cursor? Pointer { get; init; }
	public Cursor? Link { get; init; }
	public Cursor? Grab { get; init; }

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

	public void SetDragCursor(DragDropEffects effect)
	{
		Mouse.SetCursor(this.Grab);
	}

	private unsafe nint UpdateCursorDetour(RaptureAtkModule* module)
    {
		return Hooks.UpdateGameCursor.Original(module);
    }

	private IntPtr SetCursorDetour(HCURSOR hCursor)
	{
		if (this.Services.Windows.IsMouseOverWindow() || this.Services.DragAndDrop.IsDragging)
			return IntPtr.Zero;

		return Hooks.SetCursor.Original(hCursor);
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

	private void SetCursor<T>(Cursor? cursor)
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
					fe.Cursor = cursor;
				}
			}));
	}
}
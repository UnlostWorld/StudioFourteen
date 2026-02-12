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

namespace StudioFourteen.Services.Avalonia.Platform;

using global::Avalonia.Controls.Primitives;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives.PopupPositioning;
using global::Avalonia.Input;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;
using global::Avalonia.Media;
using System.Threading.Tasks;
using StudioFourteen.Services.Tick;
using System;
using global::Avalonia.Input.Raw;
using global::Avalonia.Rendering;

public class PopupImpl : WindowImpl, IPopupImpl
{
	public PopupImpl(WindowImpl owner, Compositor compositor, StudioScreens screen)
		: base(compositor, screen)
	{
		this.PopupPositioner = new ManagedPopupPositioner(
			new ManagedPopupPositionerPopupImplHelper(owner, this.MoveResize));
	}

	public IPopupPositioner? PopupPositioner { get; init; }

	public override void Show(bool activate, bool isDialog)
	{
		base.Show(false, isDialog);
	}

	public void SetWindowManagerAddShadowHint(bool enabled)
	{
	}

	public void TakeFocus()
	{
		this.InputRoot?.Focus();
	}

	public override void SetInputRoot(IInputRoot inputRoot)
	{
		base.SetInputRoot(inputRoot);

		if (inputRoot is PopupRoot vis)
		{
			// ⚠️ Hack: Wait for the root to load, then wait one more frame, then change the
			// background color ro force a redraw.
			// I have no idea why none of the invalidates seem to do this, and without this
			// the content is invisible until it changes. 😠
			Task.Run(async () =>
			{
				for (int i = 0; i < 10; i++)
				{
					do
					{
						await Task.Delay(10);
						await TickService.UiTick();
					}
					while (!vis.IsVisible);

					await Task.Delay(10);
					await TickService.UiTick();
					vis.Background = new SolidColorBrush(Colors.Pink);
					vis.Background = new SolidColorBrush(Colors.Transparent);
				}
			});
		}
	}

	private void MoveResize(PixelPoint position, Size size, double scaling)
	{
		this.Move(position);
		this.Resize(size, WindowResizeReason.Layout);
	}
}
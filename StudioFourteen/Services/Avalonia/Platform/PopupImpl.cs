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

using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Controls.Primitives.PopupPositioning;
using global::Avalonia.Platform;
using global::Avalonia.Rendering.Composition;

public class PopupImpl : WindowImpl, IPopupImpl
{
	public PopupImpl(WindowImpl owner, Compositor compositor, StudioScreens screen)
		: base(compositor, screen)
	{
		this.PopupPositioner = new ManagedPopupPositioner(
			new ManagedPopupPositionerPopupImplHelper(owner, this.MoveResize));
	}

	public override Size MaxAutoSizeHint => new Size(256, 256);

	public IPopupPositioner? PopupPositioner { get; init; }

	public void SetWindowManagerAddShadowHint(bool enabled)
	{
	}

	public void TakeFocus()
	{
		////Studio.Avalonia.MouseDevice.Capture(this);
	}

	private void MoveResize(PixelPoint position, Size size, double scaling)
	{
		this.Move(position);
		this.Resize(size, WindowResizeReason.Layout);
	}
}
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
	}

	private void MoveResize(PixelPoint position, Size size, double scaling)
	{
		this.Move(position);
		this.Resize(size, WindowResizeReason.Layout);
	}
}
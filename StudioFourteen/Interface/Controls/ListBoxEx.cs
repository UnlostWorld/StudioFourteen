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

namespace StudioFourteen.Interface.Controls;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

public class ListBoxEx : ListBox
{
	protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
	{
		return new ListBoxItemEx();
	}
}

public class ListBoxItemEx : ListBoxItem
{
	protected override void OnPointerPressed(PointerPressedEventArgs e)
	{
		PointerPoint p = e.GetCurrentPoint(this);

		if (p.Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
			return;

		base.OnPointerPressed(e);
	}

	protected override void OnPointerReleased(PointerReleasedEventArgs e)
	{
		if (e.InitialPressMouseButton == MouseButton.Right)
			return;

		base.OnPointerReleased(e);
	}
}
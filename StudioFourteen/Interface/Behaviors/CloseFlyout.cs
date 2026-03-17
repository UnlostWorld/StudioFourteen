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

namespace StudioFourteen.Interface.Behaviors;

using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using global::Avalonia;
using global::Avalonia.Data;
using global::Avalonia.Interactivity;

public class CloseFlyout : AvaloniaObject
{
	public static readonly AttachedProperty<bool> OnClickProperty =
		AvaloniaProperty.RegisterAttached<CloseFlyout, Button, bool>("OnClick", false, false, BindingMode.OneTime);

	static CloseFlyout()
	{
		OnClickProperty.Changed.AddClassHandler<Button>(HandleOnClickChanged);
	}

	public static void SetOnClick(AvaloniaObject element, bool value) => element.SetValue(OnClickProperty, value);
	public static bool GetOnClick(AvaloniaObject element) => element.GetValue(OnClickProperty);

	private static void HandleOnClickChanged(Button interactElem, AvaloniaPropertyChangedEventArgs args)
	{
		if (args.NewValue is bool value && value == true)
		{
			interactElem.AddHandler(Button.ClickEvent, Click);
		}
		else
		{
			interactElem.RemoveHandler(Button.ClickEvent, Click);
		}
	}

	private static void Click(object? source, RoutedEventArgs e)
	{
		if (source is Button button)
		{
			if (button.Command != null)
			{
				if (!button.Command.CanExecute(null))
					return;

				button.Command.Execute(null);
			}

			Popup? host = button.FindLogicalAncestorOfType<Popup>();
			host?.Close();
		}
	}
}

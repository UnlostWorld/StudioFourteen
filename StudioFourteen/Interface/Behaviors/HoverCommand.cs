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

using System.Windows.Input;

using global::Avalonia;
using global::Avalonia.Data;
using global::Avalonia.Input;
using global::Avalonia.Interactivity;

public class HoverCommand : AvaloniaObject
{
	public static readonly AttachedProperty<ICommand?> CommandProperty =
		AvaloniaProperty.RegisterAttached<HoverCommand, Interactive, ICommand?>("Command", null, false, BindingMode.OneTime);

	static HoverCommand()
	{
		CommandProperty.Changed.AddClassHandler<Interactive>(HandleCommandChanged);
	}

	public static void SetCommand(AvaloniaObject element, ICommand? commandValue) => element.SetValue(CommandProperty, commandValue);
	public static ICommand? GetCommand(AvaloniaObject element) => element.GetValue(CommandProperty);

	private static void HandleCommandChanged(Interactive interactElem, AvaloniaPropertyChangedEventArgs args)
	{
		if (args.NewValue is ICommand commandValue)
		{
			interactElem.AddHandler(InputElement.PointerEnteredEvent, Enter);
			interactElem.AddHandler(InputElement.PointerExitedEvent, Exit);
		}
		else
		{
			interactElem.RemoveHandler(InputElement.PointerEnteredEvent, Enter);
			interactElem.RemoveHandler(InputElement.PointerExitedEvent, Exit);
		}
	}

	private static void Enter(object? source, RoutedEventArgs e)
	{
		if (source is Interactive interactive)
		{
			ICommand? commandValue = interactive.GetValue(CommandProperty);
			commandValue?.Execute(true);
		}
	}

	private static void Exit(object? source, RoutedEventArgs e)
	{
		if (source is Interactive interactive)
		{
			ICommand? commandValue = interactive.GetValue(CommandProperty);
			commandValue?.Execute(false);
		}
	}
}

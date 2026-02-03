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
using Avalonia.VisualTree;

public class InspectorLink : Button
{
	public static readonly StyledProperty<string> PagePathProperty;
	public static readonly StyledProperty<object?> DestinationDataContextProperty;

	static InspectorLink()
	{
		PagePathProperty = AvaloniaProperty.Register<ContentControl, string>(nameof(ContentControl.ContentPath));
		DestinationDataContextProperty = AvaloniaProperty.Register<ContentControl, object?>(nameof(ContentControl.ContentDataContext));
	}

	public string PagePath
	{
		get => this.GetValue(PagePathProperty);
		set => this.SetValue(PagePathProperty, value);
	}

	public object? DestinationDataContext
	{
		get => this.GetValue(DestinationDataContextProperty);
		set => this.SetValue(DestinationDataContextProperty, value);
	}

	protected override void OnClick()
	{
		base.OnClick();

		Window? wnd = this.FindAncestorOfType<Window>();
		if (wnd?.DataContext is Inspector inspector)
		{
			inspector.OpenPage(this.PagePath, this.DestinationDataContext);
		}
	}
}
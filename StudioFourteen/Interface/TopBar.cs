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

namespace StudioFourteen.Interface;

using CommunityToolkit.Mvvm.ComponentModel;
using StudioFourteen.Services.Avalonia;

public partial class TopBar : ToolbarReference
{
	private readonly Hierarchy hierarchy = new();
	private readonly Inspector inspector = new();

	public TopBar()
	 : base("UI/TopBar.ui")
	{
	}

	[ObservableProperty]
	public partial bool Hierarchy { get; set; }

	[ObservableProperty]
	public partial bool Library { get; set; }

	[ObservableProperty]
	public partial bool Inspector { get; set; }

	partial void OnHierarchyChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.hierarchy.Show();
		}
		else
		{
			this.hierarchy.Hide();
		}
	}

	partial void OnLibraryChanging(bool oldValue, bool newValue)
	{
	}

	partial void OnInspectorChanged(bool oldValue, bool newValue)
	{
		if (newValue)
		{
			this.inspector.Show();
		}
		else
		{
			this.inspector.Hide();
		}
	}
}
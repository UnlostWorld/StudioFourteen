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

namespace StudioFourteen.Launcher;

using System.Collections.ObjectModel;
using System.Windows.Controls;
using DependencyPropertyGenerator;
using WpfUtils.Extensions;

public class LauncherMenu : Control
{
	public LauncherMenu()
	{
		this.Entries.Add(new("fa-Shop", "Marketplace"));
		this.Entries.Add(new("fa-Book", "Library"));

		this.Entries.Add(new("Camera", "Cameras"));
		this.Entries.Add(new("fa-CloudMoonRain", "Environment"));
		this.Entries.Add(new("fa-UserShield", "Characters"));
		this.Entries.Add(new("fa-Running", "Posing"));
		this.Entries.Add(new("fa-Lightbulb", "Lighting"));
		this.Entries.Add(new("fa-Chair", "Furniture"));
		this.Entries.Add(new("fa-Users", "Crowds"));

		this.Entries.Add(new("fa-Image", "Photo Capture"));
		////this.Entries.Add(new("fa-Save", "Save"));
		this.Entries.Add(new("fa-ObjectGroup", "Single Page Application"));

		this.Entries.Add(new("fa-History", "History"));
	}

	public FastObservableCollection<LauncherEntry> Entries { get; init; } = new();
}

public class LauncherEntry
{
	public LauncherEntry(string icon, string name)
	{
		this.Icon = icon;
		this.Name = name;
	}

	public string Icon { get; set; }
	public string? Name { get; set; }
}
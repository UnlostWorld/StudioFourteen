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

using System.Windows.Controls;
using WpfUtils.Extensions;

public class LauncherMenu : Control
{
	public LauncherMenu()
	{
		this.Entries.Add(new("fa-Shop", "LOC_Marketplace", "LOC_MarketplaceDesc", false));
		this.Entries.Add(new("fa-Book", "LOC_Library", "LOC_LibraryDesc", true));

		this.Entries.Add(new("Camera", "LOC_Camera", "LOC_CameraDesc", true));
		this.Entries.Add(new("fa-CloudMoonRain", "LOC_Environment", "LOC_EnvironmentDesc", true));
		this.Entries.Add(new("fa-UserShield", "LOC_Character", "LOC_CharacterDesc", true));
		this.Entries.Add(new("fa-Running", "LOC_Pose", "LOC_PoseDesc", true));
		this.Entries.Add(new("fa-Lightbulb", "LOC_Lighting", "LOC_LightingDesc", false));
		this.Entries.Add(new("fa-Chair", "LOC_Furniture", "LOC_FurnitureDesc", false));
		this.Entries.Add(new("fa-Users", "LOC_Crowds", "LOC_CrowdsDesc", false));
		this.Entries.Add(new("fa-Fire", "LOC_Effects", "LOC_EffectsDesc", false));

		this.Entries.Add(new("fa-Image", "LOC_Photo", "LOC_PhotoDesc", true));
		this.Entries.Add(new("fa-ObjectGroup", "LOC_Spa", "LOC_SpaDesc", true));

		this.Entries.Add(new("fa-History", "LOC_History", "LOC_HistoryDesc", true));
		this.Entries.Add(new("fa-Save", "LOC_Save", "LOC_SaveDesc", true));
		this.Entries.Add(new("fa-Cogs", "LOC_Settings", "LOC_SettingsDesc", true));
	}

	public FastObservableCollection<LauncherEntry> Entries { get; init; } = new();
}

public class LauncherEntry
{
	private readonly string nameKey;
	private readonly string descKey;

	public LauncherEntry(string icon, string nameKey, string descKey, bool enabled)
	{
		this.Icon = icon;
		this.nameKey = nameKey;
		this.descKey = descKey;
		this.IsEnabled = enabled;
	}

	public bool IsEnabled { get; set; }
	public string Icon { get; set; }
	public string? Name => Resources.Find(this.nameKey, this.nameKey);
	public string? Description => Resources.Find(this.descKey, this.descKey);
}
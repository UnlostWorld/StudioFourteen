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

using StudioFourteen.Panels;
using DependencyPropertyGenerator;
using System.Windows;
using System.Windows.Input;
using StudioFourteen.Settings;
using System.Runtime.CompilerServices;

[DependencyProperty<bool>("IsMenuOpen")]
public partial class LauncherWindow : PanelWindow
{
	public Persistence Persistence { get; init; } = new($"Panel_Launcher");

	public override T? GetPersistence<T>([CallerMemberName] string id = "")
		where T : default
		=> this.Persistence.GetPersistence<T>(id);

	public override void SetPersistence(object? value, [CallerMemberName] string id = "") => this.Persistence.SetPersistence(value, id);
	public override void SetPersistence(string id, object? value) => this.Persistence.SetPersistence(id, value);

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnLaunchClicked(object sender, RoutedEventArgs e)
	{
		if (!this.Services.Studio.IsOpen)
			this.Services.Studio.OpenStudio();

		if (!this.IsMenuOpen)
		{
			this.IsMenuOpen = true;
		}
	}
}

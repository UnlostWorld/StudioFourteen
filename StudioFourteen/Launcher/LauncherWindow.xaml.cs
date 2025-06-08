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
using System;

[DependencyProperty<bool>("IsMenuOpen")]
[DependencyProperty<bool>("IsRightSide")]
[DependencyProperty<bool>("IsBottomSide")]
[DependencyProperty<bool>("IsButtonVisible", DefaultValue = true)]
[DependencyProperty<bool>("IsStudioOpen")]
public partial class LauncherWindow : PanelWindow
{
	public static LauncherWindow? Instance;

	public LauncherWindow()
	{
		Instance = this;
		this.Services.Settings.SettingChanged += this.OnSettingChanged;
		this.Services.Studio.Opening += this.OnStudioStateChanged;
		this.Services.Studio.Closing += this.OnStudioStateChanged;
	}

	public Persistence Persistence { get; init; } = Persistence.GetPersistence($"Panel_Launcher");

	public override T? GetPersistence<T>([CallerMemberName] string id = "")
		where T : default
		=> this.Persistence.GetPersistence<T>(id);

	public override void SetPersistence(object? value, [CallerMemberName] string id = "") => this.Persistence.SetPersistence(value, id);
	public override void SetPersistence(string id, object? value) => this.Persistence.SetPersistence(id, value);

	protected override void OnLocationChanged(EventArgs e)
	{
		base.OnLocationChanged(e);
		this.IsRightSide = this.Position.X > 0.5;
		this.IsBottomSide = this.Position.Y > 0.5;
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.IsRightSide = this.Position.X > 0.5;
		this.IsBottomSide = this.Position.Y > 0.5;
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnLaunchClicked(object sender, RoutedEventArgs e)
	{
		if (!this.IsMenuOpen)
		{
			this.IsMenuOpen = true;
		}
	}

	private void OnSettingChanged(string settingName, object? newValue)
	{
		this.OnStudioStateChanged();
	}

	private void OnStudioStateChanged()
	{
		this.Dispatcher.Invoke(() =>
		{
			this.IsStudioOpen = this.Services.Studio.IsOpen;
			this.IsButtonVisible = !this.Services.Settings.Current.HideLauncherButton || this.Services.Studio.IsOpen;
		});
	}
}

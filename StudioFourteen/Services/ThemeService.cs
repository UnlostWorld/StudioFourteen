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

namespace StudioFourteen.Services;

using PropertyChanged.SourceGenerator;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfUtils.Extensions;

public partial class ThemeService : ServiceBase
{
	[Notify] private Theme? currentTheme;
	[Notify] private LauncherButton? launcher;

	public ObservableCollection<Theme> Themes { get; init; } = new();
	public ObservableCollection<LauncherButton> Launchers { get; init; } = new();

	public override Task Initialize()
	{
		this.Themes.Add(new("Dark", "pack://application:,,,/StudioFourteen;component/Themes/StudioDark.xaml"));
		this.Themes.Add(new("Light", "pack://application:,,,/StudioFourteen;component/Themes/StudioLight.xaml"));
		this.Themes.Add(new("Nier", "pack://application:,,,/StudioFourteen;component/Themes/StudioNier.xaml"));

		this.Launchers.Add(new(
			"Default",
			"pack://application:,,,/StudioFourteen;component/Assets/launcher.png",
			"pack://application:,,,/StudioFourteen;component/Launcher/Style_Default.xaml"));

		this.Launchers.Add(new(
			"Flat",
			"pack://application:,,,/StudioFourteen;component/Assets/launcher-flat.png",
			"pack://application:,,,/StudioFourteen;component/Launcher/Style_Flat.xaml"));

		this.Launchers.Add(new(
			"Paper",
			"pack://application:,,,/StudioFourteen;component/Assets/launcher-paper.png",
			"pack://application:,,,/StudioFourteen;component/Launcher/Style_Paper.xaml"));

		foreach (Theme theme in this.Themes)
		{
			if (theme.Name == this.Settings.Theme)
			{
				Resources.MergeDictionary(new(theme.Path));
				this.currentTheme = theme;
			}
		}

		foreach (LauncherButton launcher in this.Launchers)
		{
			if (launcher.Name == this.Settings.Launcher)
			{
				Resources.MergeDictionary(new(launcher.Path));
				this.launcher = launcher;
			}
		}

		return base.Initialize();
	}

	private void OnCurrentThemeChanged(Theme? oldTheme, Theme? newTheme)
	{
		try
		{
			if (newTheme == null)
				return;

			if (oldTheme != null)
				Resources.UnMergeDictionary(new(oldTheme.Path));

			this.Settings.Theme = newTheme.Name;
			Resources.MergeDictionary(new(newTheme.Path));

			this.Services.Panels.RestartPanels().Run();
		}
		catch (Exception e)
		{
			this.Log.Error(e, "changing theme");
		}
	}

	private void OnLauncherChanged(LauncherButton? oldTheme, LauncherButton? newTheme)
	{
		if (newTheme == null)
			return;

		if (oldTheme != null)
			Resources.UnMergeDictionary(new(oldTheme.Path));

		this.Settings.Launcher = newTheme.Name;
		Resources.MergeDictionary(new(newTheme.Path));

		this.Services.Panels.RestartPanels().Run();
	}
}

public class Theme(string name, string path)
{
	public string Name { get; init; } = name;
	public string Path { get; init; } = path;
}

public class LauncherButton(string name, string iconPath, string path)
{
	public string Name { get; init; } = name;
	public string IconPath { get; init; } = iconPath;
	public string Path { get; init; } = path;
}

public class Trim
{
	public Trim(string name, Color color)
	{
		this.Name = name;
		this.Color = color;
	}

	public Trim(string name, string hex)
	{
		this.Name = name;
		this.Color = (Color)ColorConverter.ConvertFromString(hex);
	}

	public string Name { get; init; }
	public Color Color { get; init; }
}
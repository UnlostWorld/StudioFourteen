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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using TerraFX.Interop.Windows;
using WpfUtils.Extensions;
using StudioFourteen.Themes;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkUIColorHolder.Delegates;

public partial class ThemeService : ServiceBase
{
	private Trim? trimColor;

	[Notify] private Theme? currentTheme;
	[Notify] private LauncherButton? launcher;
	[Notify] private List<Trim>? trimColors;

	public ObservableCollection<Theme> Themes { get; init; } = new();
	public ObservableCollection<LauncherButton> Launchers { get; init; } = new();

	public Trim? TrimColor
	{
		get => this.trimColor;
		set
		{
			lock(this)
			{
				this.trimColor = value;
				this.OnTrimColorChanged(value);
				this.RaisePropertyChanged();
			}
		}
	}

	public override Task Initialize()
	{
		this.Themes.Add(new("Dark", "pack://application:,,,/StudioFourteen;component/Themes/Dark.xaml"));
		this.Themes.Add(new("Light", "pack://application:,,,/StudioFourteen;component/Themes/Light.xaml"));
		this.Themes.Add(new("Nier", "pack://application:,,,/StudioFourteen;component/Themes/Nier.xaml"));

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

		this.LoadTrimColors(this.CurrentTheme);

		if (this.TrimColors != null)
		{
			foreach (Trim trim in this.TrimColors)
			{
				if (trim.Name == this.Settings.TrimColor)
				{
					this.TrimColor = trim;
				}
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

			Resources.Clear("TrimBrush");

			this.LoadTrimColors(newTheme);

			this.Settings.Theme = newTheme.Name;

			Resources.MergeDictionary(new(newTheme.Path));
			this.OnTrimColorChanged(this.TrimColor);

			this.Services.Panels.RestartPanels().Run();
		}
		catch (Exception e)
		{
			this.Log.Error(e, "changing theme");
		}
	}

	private void OnTrimColorChanged(Trim? newColor)
	{
		if (newColor == null)
			return;

		this.Settings.TrimColor = newColor.Name;

		Resources.Set("TrimBrush", () => new SolidColorBrush(newColor.Color));
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

	private void LoadTrimColors(Theme? newTheme)
	{
		try
		{
			lock (this)
			{
				if (newTheme == null)
					return;

				ResourceDictionary dictionary = new ResourceDictionary();
				dictionary.Source = new(newTheme.Path);
				this.TrimColors = null;

				if (!dictionary.Contains("TrimColors"))
					return;

				Array? trimColors = dictionary["TrimColors"] as Array;
				if (trimColors == null)
					return;

				string? oldTrimColor = this.Settings.TrimColor;
				Trim? newTrimColor = null;

				List<Trim> newColors = new();

				foreach (object? trimColorObject in trimColors)
				{
					if (trimColorObject == null)
						continue;

					TrimColor? trimColor = trimColorObject as TrimColor;
					if (trimColor == null || trimColor.Name == null || trimColor.Color == null)
						continue;

					Trim newTrim = new(trimColor.Name, trimColor.Color.Value);
					newColors.Add(newTrim);

					if (oldTrimColor != null && trimColor.Name == oldTrimColor)
					{
						newTrimColor = newTrim;
					}
				}

				this.TrimColors = newColors;
				if (newTrimColor == null && newColors.Count > 0)
					newTrimColor = newColors[0];

				this.TrimColor = newTrimColor;

				this.Log.Information($"new trim color {newTrimColor} --> {this.TrimColor} ?????????");
			}
		}
		catch (Exception e)
		{
			this.Log.Error(e, "caught");
		}
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
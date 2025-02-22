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
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfUtils.Extensions;
using static FFXIVClientStructs.FFXIV.Component.GUI.AtkUIColorHolder.Delegates;

public partial class ThemeService : ServiceBase
{
	[Notify] private Theme? currentTheme;
	[Notify] private Trim? trimColor;

	public ObservableCollection<Theme> Themes { get; init; } = new();
	public ObservableCollection<Trim> TrimColors { get; init; } = new();

	public override Task Initialize()
	{
		this.Themes.Add(new("Dark", "pack://application:,,,/WpfUtils;component/Themes/Dark.xaml", true));
		this.Themes.Add(new("Light", "pack://application:,,,/WpfUtils;component/Themes/Light.xaml", true));
		this.Themes.Add(new("Nier", "pack://application:,,,/WpfUtils;component/Themes/Nier.xaml", false));

		this.TrimColors.Add(new("Pink", "#FF1493"));
		this.TrimColors.Add(new("Blue", "#1484FF"));
		this.TrimColors.Add(new("Mint", "#14FFB3"));
		this.TrimColors.Add(new("Green", "#5CFF14"));
		this.TrimColors.Add(new("Yellow", "#FBFF14"));
		this.TrimColors.Add(new("Orange", "#FF7C14"));
		this.TrimColors.Add(new("Red", "#FF1414"));

		foreach (Theme theme in this.Themes)
		{
			if (theme.Name == this.Settings.Theme)
			{
				Resources.MergeDictionary(new(theme.Path));
				this.currentTheme = theme;
			}
		}

		foreach (Trim trim in this.TrimColors)
		{
			if (trim.Name == this.Settings.TrimColor)
			{
				this.TrimColor = trim;
			}
		}

		return base.Initialize();
	}

	private void OnCurrentThemeChanged(Theme? oldTheme, Theme? newTheme)
	{
		if (newTheme == null)
			return;

		if (oldTheme != null)
			Resources.UnMergeDictionary(new(oldTheme.Path));

		if (!newTheme.SupportsTrim)
			Resources.Clear("TrimBrush");

		this.Settings.Theme = newTheme.Name;
		Resources.MergeDictionary(new(newTheme.Path));
		this.OnTrimColorChanged(null, this.TrimColor);

		this.Services.Panels.RestartPanels().Run();
	}

	private void OnTrimColorChanged(Trim? oldColor, Trim? newColor)
	{
		if (newColor == null)
			return;

		this.Settings.TrimColor = newColor.Name;

		if (this.CurrentTheme?.SupportsTrim == true)
		{
			Resources.Set("TrimBrush", () => new SolidColorBrush(newColor.Color));
		}
	}
}

public class Theme(string name, string path, bool supportsTrim)
{
	public string Name { get; init; } = name;
	public string Path { get; init; } = path;
	public bool SupportsTrim { get; init; } = supportsTrim;
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
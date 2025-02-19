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
using System.Threading.Tasks;
using WpfUtils.Extensions;

public partial class ThemeService : ServiceBase
{
	[Notify] private Theme? currentTheme;

	public ObservableCollection<Theme> Themes { get; init; } = new();

	public override Task Initialize()
	{
		this.Themes.Add(new("Dark", "pack://application:,,,/WpfUtils;component/Themes/Dark.xaml"));
		this.Themes.Add(new("Light", "pack://application:,,,/WpfUtils;component/Themes/Light.xaml"));
		this.Themes.Add(new("Nier", "pack://application:,,,/WpfUtils;component/Themes/Nier.xaml"));

		foreach (Theme theme in this.Themes)
		{
			if (theme.Name == this.Settings.Theme)
			{
				Resources.MergeDictionary(new(theme.Path));
				this.currentTheme = theme;
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

		this.Settings.Theme = newTheme.Name;
		Resources.MergeDictionary(new(newTheme.Path));

		this.Services.Panels.RestartPanels().Run();
	}
}

public class Theme
{
	public Theme(string name, string path)
	{
		this.Name = name;
		this.Path = path;
	}

	public string Name { get; init; }
	public string Path { get; init; }
}
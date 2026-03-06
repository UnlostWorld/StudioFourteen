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

namespace StudioFourteen.Services.Avalonia;

using System.Collections.Generic;
using global::Avalonia;
using global::Avalonia.Controls;
using global::Avalonia.Markup.Xaml;
using StudioFourteen.Services.Content;
using StudioFourteen.Services.Tick;

public partial class StudioApplication : Application
{
	private readonly List<AvaloniaContentReference<ResourceDictionary>> resourceDictionaries = new();

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void LoadTheme()
	{
		List<string> resourceDictionaryPaths = Studio.Content.GetContents("UI/Styles/");
		resourceDictionaryPaths.Add("UI/Theme.ui");

		foreach (string resourceDictionaryPath in resourceDictionaryPaths)
		{
			AvaloniaContentReference<ResourceDictionary> resourceDictionaryReference = new(resourceDictionaryPath);
			resourceDictionaryReference.Reloaded += () => this.LoadDictionary(resourceDictionaryReference);
			this.resourceDictionaries.Add(resourceDictionaryReference);
			this.LoadDictionary(resourceDictionaryReference);
		}
	}

	private void LoadDictionary(AvaloniaContentReference<ResourceDictionary> reference)
	{
		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.Resources.MergedDictionaries.Add(reference.Get());
			Studio.Avalonia.ReloadAll();
		});
	}
}
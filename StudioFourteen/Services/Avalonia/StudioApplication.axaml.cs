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
using StudioFourteen.Services.Tick;

public partial class StudioApplication : Application
{
	private static readonly HashSet<string> ResourceDictionaryPaths = new()
	{
		"UI/Theme.ui",

		"UI/Styles/Button.ui",
		"UI/Styles/NumberBox.ui",
		"UI/Styles/Svg.ui",
		"UI/Styles/TabControl.ui",
		"UI/Styles/TabItem.ui",
		"UI/Styles/TextBlock.ui",
		"UI/Styles/TextBox.ui",
		"UI/Styles/TransformControl.ui",
		"UI/Styles/Vector3Control.ui",
	};

	private readonly List<AvaloniaContentReference<ResourceDictionary>> resourceDictionaries = new();

	public override void Initialize()
	{
		AvaloniaXamlLoader.Load(this);
	}

	public void LoadTheme()
	{
		foreach (string resourceDictionaryPath in ResourceDictionaryPaths)
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
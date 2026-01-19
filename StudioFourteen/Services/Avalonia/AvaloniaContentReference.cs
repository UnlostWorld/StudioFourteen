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

using System;
using System.IO;
using global::Avalonia;
using global::Avalonia.Markup.Xaml;
using StudioFourteen.Services.Content;

public class AvaloniaContentReference<T>(string path)
	: ContentReference<T>(path)
	where T : AvaloniaObject, new()
{
	private const string XmlNamespaces = @"
		xmlns=""https://github.com/avaloniaui""
		xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
		xmlns:studio=""clr-namespace:StudioFourteen.Services.Avalonia.Controls;assembly=StudioFourteen""
		xmlns:sys=""clr-namespace:System;assembly=mscorlib""
	";

	protected override T Load(Stream stream)
	{
		using StreamReader reader = new(stream);
		string xaml = reader.ReadToEnd();

		int endRootTag = xaml.IndexOf('>');
		xaml = xaml.Insert(endRootTag, XmlNamespaces);

		object obj = AvaloniaRuntimeXamlLoader.Load(xaml);
		if (obj is not T tObj)
			throw new Exception("Failed to load ui resource");

		return tObj;
	}
}
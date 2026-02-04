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
using System.Collections.Generic;
using System.IO;
using System.Text;
using global::Avalonia;
using global::Avalonia.Markup.Xaml;
using StudioFourteen.Services.Content;

public class AvaloniaContentReference<T>(string path)
	: ContentReference<T>(path)
	where T : AvaloniaObject, new()
{
	private static readonly string XmlNamespaces;

	static AvaloniaContentReference()
	{
		HashSet<string> namespaces = new();
		foreach (Type type in typeof(AvaloniaService).Assembly.GetTypes())
		{
			if (type.Namespace == null)
				continue;

			namespaces.Add(type.Namespace);
		}

		StringBuilder sb = new();
		sb.AppendLine(" xmlns=\"https://github.com/avaloniaui\"");
		sb.AppendLine("xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
		sb.AppendLine("xmlns:sys=\"clr-namespace:System;assembly=mscorlib\"");
		sb.AppendLine("xmlns:S14=\"clr-namespace:StudioFourteen.Interface.Controls;assembly=StudioFourteen\"");

		foreach (string ns in namespaces)
		{
			sb.Append("xmlns:");
			sb.Append(ns);
			sb.Append("=\"clr-namespace:");
			sb.Append(ns);
			sb.AppendLine(";assembly=StudioFourteen\"");
		}

		XmlNamespaces = sb.ToString();
	}

	protected override T Load(Stream stream)
	{
		using StreamReader reader = new(stream);
		string xaml = reader.ReadToEnd();

		if (string.IsNullOrEmpty(xaml))
			xaml = "<Grid></Grid>";

		int endRootTag = xaml.IndexOf('>');
		xaml = xaml.Insert(endRootTag, XmlNamespaces);

		object obj = AvaloniaRuntimeXamlLoader.Load(xaml);
		if (obj is not T tObj)
			throw new Exception("Failed to load ui resource");

		return tObj;
	}
}
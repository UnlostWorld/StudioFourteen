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

namespace StudioFourteen.Content;

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

public class XamlContentReference<T>(string path)
	: ContentReference<T>(path)
{
	private static readonly ParserContext Context = new();

	static XamlContentReference()
	{
		Context.XmlnsDictionary.Add(string.Empty, "http://schemas.microsoft.com/winfx/2006/xaml/presentation");
		Context.XmlnsDictionary.Add("x", "http://schemas.microsoft.com/winfx/2006/xaml");
		Context.XmlnsDictionary.Add("d", "http://schemas.microsoft.com/expression/blend/2008");
		Context.XmlnsDictionary.Add("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
		Context.XmlnsDictionary.Add("s14", "http://fourteen.studio");
	}

	protected override T Load(Stream stream)
	{
		List<NamespaceMapEntry> maps = new();
		maps.Add(new NamespaceMapEntry("http://fourteen.studio", "StudioFourteen", "StudioFourteen.Panels"));
		maps.Add(new NamespaceMapEntry("http://fourteen.studio", "StudioFourteen", "StudioFourteen.Controls"));
		maps.Add(new NamespaceMapEntry("http://fourteen.studio", "StudioFourteen", "StudioFourteen.Icons"));
		maps.Add(new NamespaceMapEntry("http://fourteen.studio", "WpfUtils", "WpfUtils.Controls"));

		Context.XamlTypeMapper = new(["StudioFourteen"], maps.ToArray());

		// HACK:
		// WPF Internally caches dynamic assembly lookups in the XamlTypeMapper / ReflectionHelper classes
		// however these caches are static, and will persist after StudioFourteen is reloaded by Dalamud, causing the
		// Cached version of the xaml types to be out of date, and making it impossible to cast them to the current
		// Studio versions.
		//
		// To get around this, we can use the 'SetAssemblyPath' method to flush the cache, HOWEVER, we dont actually
		// want to load the assembly by path, since Dalamud has it loaded from a MemoryStream.
		//
		// So we set the path to something that passes validation, then get the internal path field and clear it so
		// the cache gets cleared, but no path is set, allowing the internal assembly resolution flow to happen.
		{
			Context.XamlTypeMapper.SetAssemblyPath("StudioFourteen", "C:/FakePath.dll");

			FieldInfo? field = typeof(XamlTypeMapper).GetField(
				"_assemblyPathTable",
				BindingFlags.Instance | BindingFlags.NonPublic);

			if (field == null)
				throw new Exception("Failed to find _assemblyPathTable field");

			HybridDictionary? dict = field.GetValue(Context.XamlTypeMapper) as HybridDictionary;
			dict?.Clear();
		}

		T? rootElement = (T)XamlReader.Load(stream, Context);
		if (rootElement == null)
			throw new Exception($"Content \"{this.Path}\" failed to load xaml");

		if (rootElement is FrameworkElement element)
		{
			element.Resources = StudioFourteen.Resources.Load();
		}

		return rootElement;
	}
}
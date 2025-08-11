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
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Windows.Markup;

public class XamlContentReference<T>(string path)
	: ContentReference<T>(path)
{
	private static readonly ParserContext Context = new();

	protected override T Load(Stream stream)
	{
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
			Context.XamlTypeMapper = new([]);
			Context.XamlTypeMapper.SetAssemblyPath("StudioFourteen", "C:/FakePath.dll");

			FieldInfo? field = typeof(XamlTypeMapper).GetField(
				"_assemblyPathTable",
				BindingFlags.Instance | BindingFlags.NonPublic);

			HybridDictionary? dict = field?.GetValue(Context.XamlTypeMapper) as HybridDictionary;
			dict?.Clear();
		}

		T? rootElement = (T)XamlReader.Load(stream, Context);
		if (rootElement == null)
			throw new Exception($"Content \"{this.Path}\" failed to load xaml");

		return rootElement;
	}
}
// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Data;

using StudioFourteen.Serialization;
using StudioFourteen.Services;
using StudioFourteen.Posing;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

public class DataService : ServiceBase
{
	public Dictionary<string, SkeletonViewDefinition>? SkeletonViews { get; private set; }
	public Dictionary<string, BlendTarget>? ExpressionBlends { get; private set; }

	public override Task Initialize()
	{
		this.SkeletonViews = this.GetResourceDocument<Dictionary<string, SkeletonViewDefinition>>("SkeletonViews");
		this.ExpressionBlends = this.GetResourceDocument<Dictionary<string, BlendTarget>>("ExpressionBlends");

		return base.Initialize();
	}

	private Stream? GetRawResourceStream(string name)
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		string resourceName = $"StudioFourteen.Data.Documents.{name}.json";
		Stream? stream = assembly.GetManifestResourceStream(resourceName);

		if (stream == null)
			this.Log.Warning($"Embedded data document: {resourceName} not found ");

		return stream;
	}

	private T? GetResourceDocument<T>(string name)
		where T : class
	{
		using Stream? stream = this.GetRawResourceStream(name);

		if (stream == null)
			return null;

		using StreamReader reader = new StreamReader(stream);
		string txt = reader.ReadToEnd();
		T? document = Serializer.Deserialize<T>(txt);
		return document;
	}
}

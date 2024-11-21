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

namespace StudioFourteen.Serialization;

using Newtonsoft.Json;
using StudioFourteen.Serialization.Converters;
using System.Globalization;

public static class Serializer
{
	private static readonly JsonSerializerSettings Settings = new();

	static Serializer()
	{
		Settings.Culture = CultureInfo.InvariantCulture;
		Settings.Formatting = Formatting.Indented;
		Settings.NullValueHandling = NullValueHandling.Ignore;

		Settings.Converters.Add(new TagConverter());
		Settings.Converters.Add(new Vector3Converter());
		Settings.Converters.Add(new Vector3NullableConverter());
		Settings.Converters.Add(new Vector4Converter());
		Settings.Converters.Add(new Vector4NullableConverter());
		Settings.Converters.Add(new QuaternionConverter());
		Settings.Converters.Add(new QuaternionNullableConverter());
	}

	public static string Serialize(object obj)
	{
		string json = JsonConvert.SerializeObject(obj, Settings);

		if (json.StartsWith('"') && json.EndsWith('"'))
			json = json.Trim('"');

		return json;
	}

	public static T? Deserialize<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, Settings);
	}
}
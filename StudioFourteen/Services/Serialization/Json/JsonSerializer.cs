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

namespace StudioFourteen.Services.Serialization;

using Newtonsoft.Json;
using StudioFourteen.Services.Serialization.Json.Converters;
using System.Globalization;
using System.IO;

public class JsonSerializer
{
	private readonly JsonSerializerSettings settings = new();

	public JsonSerializer()
	{
		this.settings.Culture = CultureInfo.InvariantCulture;
		this.settings.Formatting = Formatting.Indented;
		this.settings.NullValueHandling = NullValueHandling.Ignore;

		this.settings.Converters.Add(new Vector2Converter());
		this.settings.Converters.Add(new Vector2NullableConverter());
		this.settings.Converters.Add(new Vector3Converter());
		this.settings.Converters.Add(new Vector3NullableConverter());
		this.settings.Converters.Add(new Vector4Converter());
		this.settings.Converters.Add(new Vector4NullableConverter());
		this.settings.Converters.Add(new QuaternionConverter());
		this.settings.Converters.Add(new QuaternionNullableConverter());
		this.settings.Converters.Add(new ColorConverter());
	}

	public void AddConverter<T>()
		where T : JsonConverter, new()
	{
		this.settings.Converters.Add(new T());
	}

	public string Serialize(object obj)
	{
		string json = JsonConvert.SerializeObject(obj, this.settings);

		if (json.StartsWith('"') && json.EndsWith('"'))
			json = json.Trim('"');

		return json;
	}

	public T? Deserialize<T>(string json)
	{
		return JsonConvert.DeserializeObject<T>(json, this.settings);
	}

	public T? Deserialize<T>(Stream stream)
	{
		using StreamReader reader = new(stream);
		return JsonConvert.DeserializeObject<T>(reader.ReadToEnd(), this.settings);
	}
}
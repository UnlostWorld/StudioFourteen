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

namespace StudioFourteen.Services.Serialization.Converters;

using Newtonsoft.Json;
using StudioFourteen.Services.Rendering;
using System;

public class ColorConverter : JsonConverter<Color>
{
	public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string ?? throw new Exception("Cannot convert null to Vector3");
		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Color v = default;
		v.R = float.Parse(parts[0], serializer.Culture);
		v.G = float.Parse(parts[1], serializer.Culture);
		v.B = float.Parse(parts[2], serializer.Culture);
		v.A = float.Parse(parts[3], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
	{
		var newString = value.R.ToString(serializer.Culture) + ", " + value.G.ToString(serializer.Culture) + ", " + value.B.ToString(serializer.Culture) + ", " + value.A.ToString(serializer.Culture);
		writer.WriteValue(newString);
	}
}
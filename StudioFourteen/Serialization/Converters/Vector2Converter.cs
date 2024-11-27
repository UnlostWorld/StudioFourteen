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

namespace StudioFourteen.Serialization.Converters;

using Newtonsoft.Json;
using System;
using System.Numerics;

public class Vector2Converter : JsonConverter<Vector2>
{
	public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string ?? throw new Exception("Cannot convert null to Vector2");
		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 2)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Vector2 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
	{
		var newString =
			value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Y.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}

public class Vector2NullableConverter : JsonConverter<Vector2?>
{
	public override Vector2? ReadJson(JsonReader reader, Type objectType, Vector2? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string;
		if (str == null)
			return null;

		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 2)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Vector2 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector2? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		var newString =
			value.Value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.Y.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}
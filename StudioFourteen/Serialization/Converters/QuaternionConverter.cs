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

public class QuaternionConverter : JsonConverter<Quaternion>
{
	public override Quaternion ReadJson(JsonReader reader, Type objectType, Quaternion existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string ?? throw new Exception("Cannot convert null to Quaternion");
		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 4)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Quaternion q = default;
		q.X = float.Parse(parts[0], serializer.Culture);
		q.Y = float.Parse(parts[1], serializer.Culture);
		q.Z = float.Parse(parts[2], serializer.Culture);
		q.W = float.Parse(parts[3], serializer.Culture);
		return q;
	}

	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
	{
		string newString =
			value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Y.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Z.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.W.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}

public class QuaternionNullableConverter : JsonConverter<Quaternion?>
{
	public override Quaternion? ReadJson(JsonReader reader, Type objectType, Quaternion? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string;
		if (str == null)
			return null;

		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 4)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Quaternion q = default;
		q.X = float.Parse(parts[0], serializer.Culture);
		q.Y = float.Parse(parts[1], serializer.Culture);
		q.Z = float.Parse(parts[2], serializer.Culture);
		q.W = float.Parse(parts[3], serializer.Culture);
		return q;
	}

	public override void WriteJson(JsonWriter writer, Quaternion? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		var newString =
			value.Value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.Y.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.Z.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.W.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}
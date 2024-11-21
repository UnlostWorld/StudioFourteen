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

namespace StudioFourteen.Serialization.Converters;

using Newtonsoft.Json;
using System;
using System.Numerics;

public class Vector4Converter : JsonConverter<Vector4>
{
	public override Vector4 ReadJson(JsonReader reader, Type objectType, Vector4 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string ?? throw new Exception("Cannot convert null to Vector3");
		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 4)
			throw new FormatException();

		Vector4 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		v.Z = float.Parse(parts[2], serializer.Culture);
		v.W = float.Parse(parts[3], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector4 value, JsonSerializer serializer)
	{
		var newString = value.X.ToString(serializer.Culture) + ", " + value.Y.ToString(serializer.Culture) + ", " + value.Z.ToString(serializer.Culture) + ", " + value.W.ToString(serializer.Culture);
		writer.WriteValue(newString);
	}
}

public class Vector4NullableConverter : JsonConverter<Vector4?>
{
	public override Vector4? ReadJson(JsonReader reader, Type objectType, Vector4? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string;
		if (str == null)
			return null;

		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 4)
			throw new FormatException($"Expected 4 components, got {parts.Length} ({str})");

		Vector4 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		v.Z = float.Parse(parts[2], serializer.Culture);
		v.W = float.Parse(parts[3], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector4? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		var newString = value.Value.X.ToString(serializer.Culture) + ", " + value.Value.Y.ToString(serializer.Culture) + ", " + value.Value.Z.ToString(serializer.Culture) + ", " + value.Value.W.ToString(serializer.Culture);
		writer.WriteValue(newString);
	}
}
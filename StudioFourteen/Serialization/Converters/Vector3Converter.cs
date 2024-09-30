// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Files/Converters/VectorConverters.cs

namespace StudioFourteen.Serialization.Converters;

using Newtonsoft.Json;
using System;
using System.Numerics;

public class Vector3Converter : JsonConverter<Vector3>
{
	public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string ?? throw new Exception("Cannot convert null to Vector3");
		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 3)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Vector3 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		v.Z = float.Parse(parts[2], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector3 value, JsonSerializer serializer)
	{
		var newString =
			value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Y.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Z.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}

public class Vector3NullableConverter : JsonConverter<Vector3?>
{
	public override Vector3? ReadJson(JsonReader reader, Type objectType, Vector3? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string;
		if (str == null)
			return null;

		string[] parts = str.Split([','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length != 3)
			throw new FormatException($"Expected 3 components, got {parts.Length} ({str})");

		Vector3 v = default;
		v.X = float.Parse(parts[0], serializer.Culture);
		v.Y = float.Parse(parts[1], serializer.Culture);
		v.Z = float.Parse(parts[2], serializer.Culture);
		return v;
	}

	public override void WriteJson(JsonWriter writer, Vector3? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		var newString =
			value.Value.X.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.Y.ToString(Formats.FloatFormat, serializer.Culture)
			+ ", "
			+ value.Value.Z.ToString(Formats.FloatFormat, serializer.Culture);
		writer.WriteValue(newString);
	}
}
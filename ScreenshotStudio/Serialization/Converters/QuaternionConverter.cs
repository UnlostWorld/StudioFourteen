// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Files/Converters/QuaternionConverter.cs

namespace ScreenshotStudio.Serialization.Converters;

using Newtonsoft.Json;
using System;
using System.Globalization;
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
		q.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		q.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		q.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		q.W = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return q;
	}

	public override void WriteJson(JsonWriter writer, Quaternion value, JsonSerializer serializer)
	{
		string newString = value.X.ToString(serializer.Culture) + ", " + value.Y.ToString(serializer.Culture) + ", " + value.Z.ToString(serializer.Culture) + ", " + value.W.ToString(serializer.Culture);
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
		q.X = float.Parse(parts[0], CultureInfo.InvariantCulture);
		q.Y = float.Parse(parts[1], CultureInfo.InvariantCulture);
		q.Z = float.Parse(parts[2], CultureInfo.InvariantCulture);
		q.W = float.Parse(parts[3], CultureInfo.InvariantCulture);
		return q;
	}

	public override void WriteJson(JsonWriter writer, Quaternion? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		var newString = value.Value.X.ToString(serializer.Culture) + ", " + value.Value.Y.ToString(serializer.Culture) + ", " + value.Value.Z.ToString(serializer.Culture) + ", " + value.Value.W.ToString(serializer.Culture);
		writer.WriteValue(newString);
	}
}
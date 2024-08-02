namespace ScreenshotStudio.Serialization;

using Newtonsoft.Json;
using ScreenshotStudio.Serialization.Converters;
using System.Globalization;

public static class Serializer
{
	private static readonly JsonSerializerSettings Settings = new();

	static Serializer()
	{
		Settings.Culture = CultureInfo.InvariantCulture;
		Settings.Formatting = Formatting.Indented;

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
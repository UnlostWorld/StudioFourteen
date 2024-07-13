namespace ScreenshotStudio.Serialization;

using Newtonsoft.Json;
using System.Globalization;
using System.IO;

public static class Serializer
{
	private static readonly JsonSerializerSettings Settings = new();

	static Serializer()
	{
		Settings.Culture = CultureInfo.InvariantCulture;
		Settings.Formatting = Formatting.Indented;
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
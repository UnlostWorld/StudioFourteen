namespace ScreenshotStudio.Serialization.Converters;

using Newtonsoft.Json;
using ScreenshotStudio.Tags;
using System;

public class TagConverter : JsonConverter<Tag>
{
	public override Tag? ReadJson(JsonReader reader, Type objectType, Tag? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? tagString = reader.Value as string;
		if (tagString == null)
			return null;

		return Tag.Get(tagString);
	}

	public override void WriteJson(JsonWriter writer, Tag? value, JsonSerializer serializer)
	{
		writer.WriteValue(value?.Name);
	}
}

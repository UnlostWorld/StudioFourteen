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
using StudioFourteen.Tags;
using System;
using System.Text;

public class TagCollectionConverter : JsonConverter<TagCollection>
{
	public override TagCollection? ReadJson(JsonReader reader, Type objectType, TagCollection? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? tagString = reader.Value as string;
		if (tagString == null)
			return null;

		TagCollection tags = new();
		string[] tagNames = tagString.Split(", ", StringSplitOptions.RemoveEmptyEntries);
		foreach(string tagName in tagNames)
		{
			tags.Add(tagName);
		}

		return tags;
	}

	public override void WriteJson(JsonWriter writer, TagCollection? value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteValue((string?)null);
			return;
		}

		StringBuilder builder = new();
		for(int i = 0; i < value.Count; i++)
		{
			if (i > 0)
				builder.Append(", ");

			builder.Append(value[i].Name);
		}

		writer.WriteValue(builder.ToString());
	}
}

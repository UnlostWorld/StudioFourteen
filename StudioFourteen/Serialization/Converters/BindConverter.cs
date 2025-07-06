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
using StudioFourteen.Input;
using System;
using System.Text;

public class BindConverter : JsonConverter<Bind>
{
	public override Bind? ReadJson(JsonReader reader, Type objectType, Bind? existingValue, bool hasExistingValue, JsonSerializer serializer)
	{
		string? str = reader.Value as string;
		if (str == null)
			return null;

		string[] parts = str.Split([" + "], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

		if (parts.Length < 1)
			throw new Exception($"Invalid bind string: {str}");

		Bind b = new();
		b.PrimaryAxis = parts[0];
		b.ModifierAxes = new (parts[1..]);
		return b;
	}

	public override void WriteJson(JsonWriter writer, Bind? value, JsonSerializer serializer)
	{
		if (value == null)
			return;

		StringBuilder b = new();
		b.Append(value.PrimaryAxis);
		foreach (string modifier in value.ModifierAxes)
		{
			b.Append(" + ");
			b.Append(modifier);
		}

		writer.WriteValue(b.ToString());
	}
}
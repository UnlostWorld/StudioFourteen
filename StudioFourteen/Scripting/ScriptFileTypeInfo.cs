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

namespace StudioFourteen.Scripting;

using System;
using System.IO;
using System.Text;
using Serilog.Events;
using StudioFourteen.Files;
using StudioFourteen.Utils;
using StudioFourteen.Controls;

public class ScriptFileTypeInfo : FileTypeInfoBase
{
	public override string Extension => ".s14script";
	public override string TypeName => "Script";
	public override object? Icon => Resources.Find("ICON_Library_Entry_Script");

	public override Type LoadsType => typeof(ScriptFile);

	public override FileBase? Load(FileInfo fileInfo)
	{
		if (!fileInfo.Exists)
			return null;

		string name = Path.GetFileNameWithoutExtension(fileInfo.FullName);
		string text = File.ReadAllText(fileInfo.FullName);
		string hash = HashUtility.GetHashString(text);

		ScriptFile script = new(fileInfo, hash);

		int jsonStart = 0;
		int jsonEnd = 0;
		int jsonBrackets = 0;

		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '{')
			{
				if (jsonBrackets == 0)
					jsonStart = i;

				jsonBrackets++;
			}
			else if (text[i] == '}')
			{
				jsonBrackets--;

				if (jsonBrackets == 0)
				{
					jsonEnd = i + 1;
					break;
				}
			}
		}

		string json = text.Substring(jsonStart, jsonEnd - jsonStart);
		script.Code = text.Substring(jsonEnd, text.Length - jsonEnd);

		if (string.IsNullOrEmpty(json))
		{
			script.Diagnostics.Add(new(LogEventLevel.Error, "No header in script file", null));
			script.HasErrors = true;
			return script;
		}

		ScriptHeader? header = null;
		try
		{
			header = Serialization.Serializer.Deserialize<ScriptHeader>(json);
		}
		catch (Exception ex)
		{
			script.Diagnostics.Add(new(LogEventLevel.Error, ex.Message, null));
			script.HasErrors = true;
			return script;
		}

		if (header != null)
		{
			script.Title = header.Title;
			script.Author = header.Author;
			script.Description = header.Description;
			script.Version = header.Version;
			script.Base64Image = header.Base64Image;
			script.Tags = header.Tags;
			script.Options = header.Options;
		}

		return script;
	}
}

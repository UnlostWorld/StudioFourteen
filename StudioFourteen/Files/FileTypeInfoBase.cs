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

namespace StudioFourteen.Files;

using Serilog;
using System;
using System.IO;

public abstract class FileTypeInfoBase
{
	public ILogger Log => Logging.ForContext(this.GetType());

	public abstract string Extension { get; }
	public abstract string TypeName { get; }
	public abstract Type LoadsType { get; }

	public abstract FileBase? Load(FileInfo fileInfo);
}

public abstract class JsonFileTypeInfoBase<T> : FileTypeInfoBase
	where T : FileBase, new()
{
	public override Type LoadsType => typeof(T);

	public override FileBase? Load(FileInfo fileInfo)
	{
		if (!fileInfo.Exists)
			return null;

		string json = File.ReadAllText(fileInfo.FullName);
		return Serialization.Serializer.Deserialize<T>(json);
	}
}
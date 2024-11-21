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

namespace StudioFourteen.Online;

using StudioFourteen.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;

public class OnlineJsonFile<T> : OnlineFile
{
	private T? data;

	public OnlineJsonFile(string url, TimeSpan updateFrequency)
		: base(url, updateFrequency)
	{
	}

	public OnlineJsonFile(string url, int version = 1)
		: base(url, version)
	{
	}

	public async Task<T> GetAsync()
	{
		if (this.CurrentState == States.None || this.data == null)
		{
			using FileStream file = await this.GetFileAsync();
			using StreamReader streamReader = new(file);
			string json = await streamReader.ReadToEndAsync();
			T? obj = Serializer.Deserialize<T>(json);

			if (obj == null)
				throw new Exception($"Failed to deserialize online file {this.Url} to type {typeof(T)}");

			this.data = obj;
		}

		return this.data;
	}
}

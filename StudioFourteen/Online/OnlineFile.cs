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

using Serilog;
using StudioFourteen.Utils;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

public class OnlineFile
{
	public readonly string Url;

	protected readonly ILogger Log = Logging.ForContext<OnlineFile>();
	protected readonly TimeSpan? updateFrequency;
	protected readonly int version;

	public OnlineFile(string url, TimeSpan updateFrequency)
	{
		this.Url = url;
		this.updateFrequency = updateFrequency;
		this.version = 1;
	}

	public OnlineFile(string url, int version = 1)
	{
		this.Url = url;
		this.version = version;
	}

	public enum States
	{
		None,
		Updating,
		Cached,
		Updated,
		Error,
	}

	public States CurrentState { get; private set; }

	protected ServiceManager Services => ServiceManager.Instance;

	public FileStream? GetFile()
	{
		if (this.CurrentState == States.None)
			throw new Exception("Attempt to access online file before it has been cached");

		if (this.CurrentState == States.Updating)
			throw new Exception("Attempt to access online file while it is being cached");

		return new FileStream(this.GetCachePath(), FileMode.Open);
	}

	public async Task<FileStream> GetFileAsync()
	{
		while (this.CurrentState == States.Updating)
			await Task.Delay(100);

		if (this.CurrentState == States.None)
		{
			this.CurrentState = await this.Update();
		}

		return new FileStream(this.GetCachePath(), FileMode.Open);
	}

	public string GetCachePath()
	{
		if (this.Services.Online.FileCache == null)
			throw new Exception("No file cache directory set");

		string file = HashUtility.GetHashString(this.Url);
		return $"{this.Services.Online.FileCache.FullName}/{file}-v{this.version}.file";
	}

	private async Task<States> Update()
	{
		string cachePath = this.GetCachePath();

		if (File.Exists(cachePath))
		{
			// Files with an update frequency will be re-downloaded if they are older than the
			// update frequency.
			if (this.updateFrequency != null)
			{
				DateTime lastWrite = File.GetLastWriteTimeUtc(cachePath);
				if (lastWrite + this.updateFrequency < DateTime.UtcNow)
				{
					return States.Cached;
				}
			}

			// Files without an update frequency are only downloaded once. To re-download them
			// increment the version index to get a new cache, or empty the cache.
			else
			{
				return States.Cached;
			}
		}

		try
		{
			this.Log.Information($"Downloading online file {this.Url}");

			using var client = new HttpClient();
			using HttpResponseMessage response = await client.GetAsync(this.Url);
			using FileStream file = new(cachePath, FileMode.OpenOrCreate);
			await response.Content.CopyToAsync(file);
			return States.Updated;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error updating online file");
			return States.Error;
		}
	}
}
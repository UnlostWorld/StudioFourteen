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

namespace StudioFourteen.Services.Content;

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using StudioFourteen.Services.Tick;

public class ContentService : IService
{
	private readonly Dictionary<string, HashSet<ContentReference>> references = new();

	private readonly string? contentDirectory;

	public ContentService()
	{
		FileInfo? assembly = Studio.PluginInterface.AssemblyLocation;
		if (assembly != null)
		{
			// Check to see if the content directory is up one for when we are running
			// from editor
			this.contentDirectory = Path.GetFullPath($"{assembly.DirectoryName}/../Content/");

			if (!Path.Exists(this.contentDirectory))
			{
				this.contentDirectory = Path.GetFullPath($"{assembly.DirectoryName}/Content/");
			}
		}

		Studio.Tick.Add(TickChannels.Studio, this.OnTick);
	}

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.Studio, this.OnTick);
	}

	public List<string> GetContents(string directoryPath)
	{
		string resolvedPath = this.ResolvePath(directoryPath);
		string[] paths = Directory.GetFiles(resolvedPath, "*.*", SearchOption.AllDirectories);

		List<string> results = new();
		foreach (string path in paths)
		{
			string cleanPath = path.Replace(resolvedPath, string.Empty);
			cleanPath = directoryPath + "/" + cleanPath;
			results.Add(cleanPath);
		}

		return results;
	}

	public Stream GetContent(ContentReference reference)
	{
		lock (this.references)
		{
			string resolvedPath = this.ResolvePath(reference.Path);

			if (!this.references.ContainsKey(resolvedPath))
				this.references.Add(resolvedPath, new());

			this.references[resolvedPath].Add(reference);
		}

		reference.LastLoadTimeUtc = DateTime.UtcNow;
		return this.GetContent(reference.Path);
	}

	public Stream GetContent(string path)
	{
		string resolvedPath = this.ResolvePath(path);

		FileStream? stream = null;
		for (int i = 0; i < 10; i++)
		{
			try
			{
				stream = new(resolvedPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			}
			catch (IOException)
			{
				Thread.Sleep(10);
			}
		}

		if (stream == null)
			throw new Exception($"Content \"{resolvedPath}\" not found");

		return stream;
	}

	public string ResolvePath(string path)
	{
		// TODO: Support overload packs in directories other than Studio.
		return Path.GetFullPath($"{this.contentDirectory}/Studio/{path}");
	}

	private void OnTick()
	{
		lock (this.references)
		{
			foreach ((string path, HashSet<ContentReference> references) in this.references)
			{
				FileInfo info = new FileInfo(path);

				foreach (ContentReference reference in references)
				{
					if (info.LastWriteTimeUtc > reference.LastLoadTimeUtc + TimeSpan.FromMilliseconds(500))
					{
						reference.LastLoadTimeUtc = info.LastWriteTimeUtc;
						reference.Reload();
					}
				}
			}
		}
	}
}

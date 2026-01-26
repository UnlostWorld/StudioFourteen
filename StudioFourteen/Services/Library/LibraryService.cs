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

namespace StudioFourteen.Services.Library;

using StudioFourteen.Services.Library.Files;
using StudioFourteen.Services.Library.Filters;
using StudioFourteen.Services.Library.Results;
using StudioFourteen.Services.Library.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

public enum PreviewModes
{
	Temporary,
	Permanent,
	Disabled,
}

public class LibraryService : IService
{
	public readonly FileTypes FileTypes;
	public readonly FileThumbnails Thumbnails;

	private readonly LibraryRoot rootItem = new();
	private readonly List<SourceBase> sources = new();

	public LibraryService()
	{
		this.FileTypes = new();
		this.Thumbnails = new();

		this.AddSource(new FileSource("Studio Fourteen", this.StudioFourteenDir));
		this.AddSource(new FileSource("Brio", this.BrioDir));
		this.AddSource(new FileSource("Anamnesis", this.AnamnesisDir));
		this.AddSource(new FileSource("Ktisis", this.KtisisDir));
		this.AddSource(new FileSource("Mare", this.MareDir));
		this.AddSource(new FileSource("Anamnesis Standard Poses", this.AnamnesisStandardDir));

		this.LoadSources();
	}

	public DirectoryInfo StudioFourteenAppDataDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/StudioFourteen/");
	public DirectoryInfo StudioFourteenDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/StudioFourteen/");
	public DirectoryInfo BrioDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Brio/");
	public DirectoryInfo AnamnesisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Anamnesis/");
	public DirectoryInfo KtisisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Ktisis/");
	public DirectoryInfo MareDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Mare/");
	public DirectoryInfo AnamnesisStandardDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Anamnesis/StandardPoses/");

	public bool IsLoadingSources { get; private set; }
	public GroupEntryBase Root => this.rootItem;

	public IEnumerable<SourceBase> Sources => this.sources;

	public bool IsScanning
	{
		get
		{
			foreach (SourceBase source in this.sources)
			{
				if (source.IsScanning)
				{
					return true;
				}
			}

			return false;
		}
	}

	public void Dispose()
	{
		Tag.ClearTagCache();

		foreach (SourceBase source in this.sources)
		{
			source.Dispose();
		}

		this.rootItem.Clear();
		this.sources.Clear();

		this.Thumbnails.Dispose();
	}

	public void AddSource(SourceBase source)
	{
		this.sources.Add(source);
	}

	public void LoadSources()
	{
		this.IsLoadingSources = true;

		this.rootItem.Clear();

		foreach (SourceBase source in this.sources)
		{
			this.rootItem.Add(source);
		}

		this.Scan();

		this.IsLoadingSources = false;
	}

	public void Scan()
	{
		Task.Run(this.ScanAsync);
	}

	public async Task ScanAsync()
	{
		lock (this)
		{
			if (this.IsScanning)
			{
				return;
			}
		}

		try
		{
			Stopwatch sw = new();
			sw.Start();

			List<Task> scanTasks = new();
			foreach (SourceBase source in this.sources)
			{
				scanTasks.Add(Task.Run(() => this.ScanSource(source)));
			}

			await Task.WhenAll(scanTasks.ToArray());

			sw.Stop();
			Studio.Log.Information($"Scanned {this.sources.Count} library sources in {sw.ElapsedMilliseconds}ms");
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error during library scan");
		}
	}

	public List<T> GetSources<T>()
		where T : SourceBase
	{
		List<T> results = new();

		foreach (SourceBase source in this.sources)
		{
			if (source is T tSource)
			{
				results.Add(tSource);
			}
		}

		return results;
	}

	public T? GetSource<T>()
		where T : SourceBase
	{
		foreach (SourceBase source in this.sources)
		{
			if (source is T tSource)
			{
				return tSource;
			}
		}

		return null;
	}

	public List<T> GetAll<T>()
		where T : LibraryEntryBase
	{
		List<FilterBase> filters = new List<FilterBase>();
		filters.Add(new TypeFilter(typeof(T)));

		GroupResult group = new(this.Root);
		group.FilterEntries(filters.ToArray());
		IEnumerable<Result>? results = group.Get(true);

		List<T> finalResults = new();
		if (results == null)
			return finalResults;

		foreach (Result result in results)
		{
			if (result.Entry is T tEntry)
			{
				finalResults.Add(tEntry);
			}
		}

		return finalResults;
	}

	private void OnConfigurationChanged()
	{
		if (this.IsLoadingSources || this.IsScanning)
			return;

		this.LoadSources();
		this.Scan();
	}

	private void ScanSource(SourceBase source)
	{
		try
		{
			source.ScanSource();
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, $"Error in library source: {source.Name}");
		}
	}
}

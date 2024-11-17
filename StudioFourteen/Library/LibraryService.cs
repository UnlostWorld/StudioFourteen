namespace StudioFourteen.Library;

using StudioFourteen.Library.Sources;
using StudioFourteen.Services;
using StudioFourteen.Tags;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public class LibraryService : ServiceBase
{
	private readonly LibraryRoot rootItem = new();
	private readonly List<SourceBase> sources = new();

	public delegate void OnScanCompleteDelegate();
	public event OnScanCompleteDelegate? ScanComplete;

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

	public void AddSource(SourceBase source)
	{
		this.sources.Add(source);
	}

	public override Task Start()
	{
		this.AddSource(new FileSource("Studio Fourteen", this.Services.Files.StudioFourteenDir));
		this.AddSource(new FileSource("Brio", this.Services.Files.BrioDir));
		this.AddSource(new FileSource("Anamnesis", this.Services.Files.AnamnesisDir));
		this.AddSource(new FileSource("Ktisis", this.Services.Files.KtisisDir));
		this.AddSource(new FileSource("Mare", this.Services.Files.MareDir));
		this.AddSource(new GlamourerSource());

		this.LoadSources();
		return base.Start();
	}

	public override Task Stop()
	{
		Tag.ClearTagCache();

		foreach (SourceBase source in this.sources)
		{
			source.Dispose();
		}

		this.rootItem.Clear();
		this.sources.Clear();

		return base.Stop();
	}

	public void LoadSources()
	{
		this.IsLoadingSources = true;

		this.rootItem.Clear();

		foreach(SourceBase source in this.sources)
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
			foreach(SourceBase source in this.sources)
			{
				scanTasks.Add(Task.Run(() => this.ScanSource(source)));
			}

			await Task.WhenAll(scanTasks.ToArray());

			sw.Stop();
			this.Log.Information($"Scanned {this.sources.Count} library sources in {sw.ElapsedMilliseconds}ms");
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error during library scan");
		}

		this.NotifyScanComplete();
	}

	public void NotifyScanComplete()
	{
		if (this.IsScanning)
			return;

		this.ScanComplete?.Invoke();
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
		catch(Exception ex)
		{
			this.Log.Error(ex, $"Error in library source: {source.Name}");
		}
	}
}

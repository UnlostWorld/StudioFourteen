namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tags;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LibraryService : ServiceBase
{
	private readonly LibraryRoot rootItem = new();
	private readonly List<SourceBase> sources = new();

	public delegate void OnScanFinishedDelegate();
	public event OnScanFinishedDelegate? OnScanFinished;

	public bool IsScanning { get; private set; }
	public bool IsLoadingSources { get; private set; }
	public GroupEntryBase Root => this.rootItem;

	public void AddSource(SourceBase source)
	{
		this.sources.Add(source);
	}

	public override Task Start()
	{
		this.AddSource(new FileSource("Characters", Environment.SpecialFolder.MyDocuments, "/ScreenshotStudio/Characters/"));
		this.AddSource(new FileSource("Poses", Environment.SpecialFolder.MyDocuments, "/ScreenshotStudio/Poses/"));

		this.AddSource(new FileSource("Brio Characters", Environment.SpecialFolder.MyDocuments, "/Brio/Characters/"));
		this.AddSource(new FileSource("Brio Poses", Environment.SpecialFolder.MyDocuments, "/Brio/Poses/"));

		this.AddSource(new FileSource("Anamnesis Characters", Environment.SpecialFolder.MyDocuments, "/Anamnesis/Characters/"));
		this.AddSource(new FileSource("Anamnesis Poses", Environment.SpecialFolder.MyDocuments, "/Anamnesis/Poses/"));

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
			if(this.IsScanning)
				return;

			this.IsScanning = true;
		}

		try
		{
			List<Task> scanTasks = new();
			foreach(SourceBase source in this.sources)
			{
				scanTasks.Add(Task.Run(() => this.ScanSource(source)));
			}

			await Task.WhenAll(scanTasks.ToArray());
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error during library scan");
		}

		this.IsScanning = false;
		this.OnScanFinished?.Invoke();
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
			source.Clear();
			source.Scan();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, $"Error in library source: {source.Name}");
		}
	}
}

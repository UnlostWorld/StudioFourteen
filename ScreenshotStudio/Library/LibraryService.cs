namespace ScreenshotStudio.Library;

using ScreenshotStudio.Library.Filters;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Tags;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ScreenshotStudio.Services;

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

		return base.Stop();
	}

	/*public void ShowFilePicker(FilterBase filter, Action<object> callback)
	{
		string title = $"Import {filter.Name}###import_browse";

		// Build the filter string for Dalamud's file picker
		// "Pose File (*.pose | *.cmp){.pose,.cmp}"
		StringBuilder filterBuilder = new();
		StringBuilder typeIdBuilder = new();

		if(filter is TypeFilter typeFilter)
		{
			List<FileTypeInfoBase> allInfos = new();
			foreach(Type filterType in typeFilter.Types)
			{
				FileTypeInfoBase? typeInfo = _fileService.GetFileTypeInfo(filterType);
				if(typeInfo == null)
					continue;

				allInfos.Add(typeInfo);
			}

			foreach(FileTypeInfoBase typeInfo in allInfos)
			{
				typeIdBuilder.Append(typeInfo.Extension);
			}

			filterBuilder.Append("Any File(");
			for(int i = 0; i < allInfos.Count; i++)
			{
				if(i > 0)
					filterBuilder.Append(" | ");

				filterBuilder.Append("*");
				filterBuilder.Append(allInfos[i].Extension);
			}

			filterBuilder.Append("){");
			foreach(FileTypeInfoBase typeInfo in allInfos)
			{
				filterBuilder.Append(typeInfo.Extension);
				filterBuilder.Append(",");
			}
			filterBuilder.Append("},");

			foreach(FileTypeInfoBase typeInfo in allInfos)
			{
				filterBuilder.Append(",");
				filterBuilder.Append(typeInfo.Name);
				filterBuilder.Append(" (*");
				filterBuilder.Append(typeInfo.Extension);
				filterBuilder.Append("){");
				filterBuilder.Append(typeInfo.Extension);
				filterBuilder.Append("}");

			}
		}

		// Add the file source directories as shortcuts
		UIManager.Instance.FileDialogManager.CustomSideBarItems.Clear();
		foreach(SourceBase source in this.sources)
		{
			if(source is FileSource fs)
			{
				if(!Directory.Exists(fs.DirectoryPath))
					continue;

				UIManager.Instance.FileDialogManager.CustomSideBarItems.Add((fs.Name, fs.DirectoryPath, FontAwesomeIcon.FolderClosed, 0));
			}
		}

		// Get the last directory the user used for these types
		string typesId = typeIdBuilder.ToString();
		string? lastDirectory = null;
		var lastDirectories = _configurationService.Configuration.Library.LastBrowsePaths;
		lastDirectories.TryGetValue(typesId, out lastDirectory);

		// Show the dalamud file picker
		UIManager.Instance.FileDialogManager.OpenFileDialog(
			title,
			filterBuilder.ToString(),
			(success, paths) =>
			{
				if(success && paths.Count == 1)
				{
					var path = paths[0];
					object? result = _fileService.Load(path);

					if(result == null)
						return;

					string? dir = Path.GetDirectoryName(path);

					if(dir != null)
					{
						if(!lastDirectories.ContainsKey(typesId))
							lastDirectories.Add(typesId, dir);

						lastDirectories[typesId] = dir;
						_configurationService.Save();
					}

					callback.Invoke(result);
				}
			},
			1,
			lastDirectory,
			true);
	}*/

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

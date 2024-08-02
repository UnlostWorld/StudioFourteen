namespace ScreenshotStudio.Library.Sources;

using ScreenshotStudio.Files;
using ScreenshotStudio.Library.Executors;
using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using static System.Environment;

public class FileSource : SourceBase
{
	private readonly string name;
	private readonly DirectoryInfo? directory;

	public FileSource(string name, string directory)
	{
		this.name = name;
		this.directory = new(directory);
	}

	public FileSource(string name, DirectoryInfo? directory)
	{
		this.name = name;
		this.directory = directory;
	}

	public FileSource(string name, SpecialFolder folder, string path)
	{
		this.name = name;

		path = $"{Environment.GetFolderPath(folder)}/{path}";
		this.directory = new(path);
	}

	public override string Name => this.name;

	public override void Scan()
	{
		if (this.directory == null)
			return;

		this.ScanDirectory(this.directory, this);
	}

	protected override string GetInternalId() => $"File_{this.name}";

	private void ScanDirectory(DirectoryInfo directory, GroupEntryBase parent)
	{
		if (!directory.Exists)
			return;

		DirectoryInfo[] dirs = directory.GetDirectories();
		foreach (DirectoryInfo dir in dirs)
		{
			DirectoryEntry dirEntry = new(this, dir);
			parent.Add(dirEntry);

			this.ScanDirectory(dir, dirEntry);
		}

		FileInfo[] files = directory.GetFiles();
		foreach (FileInfo file in files)
		{
			FileTypeInfoBase? typeInfo = FileTypes.GetTypeInfo(file);
			if (typeInfo == null)
				continue;

			parent.Add(new FileEntry(this, file, typeInfo));
		}
	}
}

public class FileEntry : LibraryEntryBase<FileEntryExecutor>
{
	private readonly FileInfo fileInfo;
	private string? iconPath;
	private bool hasGeneratedIcon;

	public FileEntry(FileSource source, FileInfo file, FileTypeInfoBase typeInfo)
		: base(source)
	{
		this.fileInfo = file;
		this.TypeInfo = typeInfo;

		try
		{
			FileBase? fileBase = typeInfo.Load(file);
			if (fileBase != null)
			{
				fileBase.GetAutoTags(this.Tags);
				this.Tags.Add(fileBase.Tags);
			}
		}
		catch(Exception ex)
		{
			// broken file!
			this.Log.Warning(ex, $"Failed to load file: {file}");
		}
	}

	public FileTypeInfoBase TypeInfo { get; init; }

	public override string Name => this.fileInfo.Name;

	public FileBase? File => this.TypeInfo.Load(this.fileInfo);

	public string? Author => this.File?.Author;
	public string? Description => this.File?.Description;
	public string? Version => this.File?.Version;
	public ImageSource? Image => this.File?.GetImage();

	public string? IconPath
	{
		get
		{
			if (!this.hasGeneratedIcon)
			{
				this.hasGeneratedIcon = true;
				ServiceManager.Instance.Thumbnails.GetThumbnail(this.fileInfo, this.OnThumbnailGenerated);
			}

			return this.iconPath;
		}
	}

	public override bool IsType(Type type)
	{
		if (base.IsType(type))
			return true;

		return this.TypeInfo.LoadsType.IsAssignableTo(type);
	}

	protected override string GetInternalId() => this.fileInfo.FullName;

	private void OnThumbnailGenerated(string path)
	{
		this.iconPath = path;
		this.NotifyPropertyChanged(nameof(FileEntry.IconPath));
	}
}

public class DirectoryEntry : GroupEntryBase
{
	private readonly DirectoryInfo directory;

	public DirectoryEntry(FileSource source, DirectoryInfo directory)
		: base(source)
	{
		this.directory = directory;
	}

	public override string Name => this.directory.Name;
	protected override string GetInternalId() => this.directory.FullName;
}
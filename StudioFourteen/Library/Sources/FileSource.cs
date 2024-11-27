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

namespace StudioFourteen.Library.Sources;

using StudioFourteen.Files;
using StudioFourteen.Library.Filters;
using StudioFourteen.Library.LibraryMenu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfUtils.Utils;

using static System.Environment;

public class FileSource : SourceBase
{
	public readonly DirectoryInfo? Directory;

	private readonly string name;

	private FuncQueue? scanQueue;
	private FileSystemWatcher? watcher;

	public FileSource(string name, string directory)
	{
		this.name = name;
		this.Directory = new(directory);
	}

	public FileSource(string name, DirectoryInfo? directory)
	{
		this.name = name;
		this.Directory = directory;
	}

	public FileSource(string name, SpecialFolder folder, string path)
	{
		this.name = name;

		path = $"{Environment.GetFolderPath(folder)}/{path}";
		this.Directory = new(path);
	}

	public override string Name => this.name;

	public override void Dispose()
	{
		base.Dispose();

		this.watcher?.Dispose();
	}

	protected override void Scan()
	{
		if (this.Directory == null)
			return;

		if (this.scanQueue == null)
		{
			this.scanQueue = new(this.ScanSource, 200);
		}

		if (this.watcher == null && this.Directory.Exists)
		{
			this.watcher = new(this.Directory.FullName);
			this.watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.Attributes;
			this.watcher.Filter = "*.*";
			this.watcher.Changed += (s, e) => this.scanQueue.Invoke();
			this.watcher.Deleted += (s, e) => this.scanQueue.Invoke();
			this.watcher.EnableRaisingEvents = true;
		}

		this.ScanDirectory(this.Directory, this);
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
			FileTypeInfoBase? typeInfo = this.Services.Files.GetTypeInfo(file);
			if (typeInfo == null)
				continue;

			parent.Add(new FileEntry(this, file, typeInfo));
		}
	}
}

public class FileEntry : LibraryEntryBase
{
	private readonly FileInfo fileInfo;
	private string? iconPath;
	private bool hasGeneratedIcon;

	public FileEntry(SourceBase source, FileInfo file, FileTypeInfoBase typeInfo)
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

	public override string Name => Path.GetFileNameWithoutExtension(this.fileInfo.Name);
	public override string? SubTitle => this.fileInfo.DirectoryName;

	public FileBase? File
	{
		get
		{
			FileBase? file = this.TypeInfo.Load(this.fileInfo);
			if (file == null)
				return null;

			if (string.IsNullOrEmpty(file.Title))
			{
				file.Title = Path.GetFileNameWithoutExtension(this.fileInfo.Name);
			}

			return file;
		}
	}

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

	public override async Task GetLibraryMenus(ILibraryContextMenu menu)
	{
		await base.GetLibraryMenus(menu);

		FileBase? file = this.File;
		if (file != null)
		{
			Type type = file.GetType();
			MethodInfo[] methods = type.GetMethods();
			foreach (MethodInfo method in methods)
			{
				LibraryMenuAttributeBase? attribute = method.GetCustomAttribute<LibraryMenuAttributeBase>();
				if (attribute == null)
					continue;

				await attribute.GetMenu(file, method, menu);
			}
		}
	}

	public override LibraryPreviewBase? GetPreview()
	{
		FileBase? file = this.File;
		if (file != null)
			return file.GetPreview();

		return base.GetPreview();
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
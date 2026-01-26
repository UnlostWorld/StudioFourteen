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

namespace StudioFourteen.Services.Library.Files;

using System;
using System.IO;
using System.Threading.Tasks;

using static System.Environment;

public class FileSource : SourceBase
{
	public readonly DirectoryInfo? Directory;

	private readonly string name;

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
	}

	public FileEntry Get(FileInfo fileInfo, FileTypeInfoBase typeInfo)
	{
		return new(this, fileInfo, typeInfo);
	}

	protected override void Scan()
	{
		if (this.Directory == null)
			return;

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
			if (Studio.IsDisposed)
				return;

			FileTypeInfoBase? typeInfo = Studio.Library.FileTypes.GetTypeInfo(file);
			if (typeInfo == null)
				continue;

			parent.Add(this.Get(file, typeInfo));
		}
	}
}

public class FileEntry : LibraryEntryBase
{
	private readonly FileInfo fileInfo;

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
		catch (Exception ex)
		{
			// broken file!
			Studio.Log.Warning(ex, $"Failed to load file: {file}");
		}
	}

	public FileTypeInfoBase TypeInfo { get; init; }

	public override string Name => Path.GetFileNameWithoutExtension(this.fileInfo.Name);
	public override string? SubTitle => this.fileInfo.DirectoryName;

	/*public override object? Icon
	{
		get
		{
			object? typeIcon = this.TypeInfo.Icon;
			if (typeIcon != null)
				return new ThumbnailIcon(this.fileInfo, typeIcon);

			return new ThumbnailIcon(this.fileInfo, XamlResources.Find("ICON_Library_Entry"));
		}
	}*/

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
	////public ImageSource? Image => this.File?.GetImage();

	public override bool IsType(Type type)
	{
		if (base.IsType(type))
			return true;

		return this.TypeInfo.LoadsType.IsAssignableTo(type);
	}

	public override LibraryPreviewBase? GetPreview()
	{
		FileBase? file = this.File;
		if (file != null)
			return file.GetPreview();

		return base.GetPreview();
	}

	public override Task Execute()
	{
		FileBase? file = this.File;

		if (file == null)
			return Task.CompletedTask;

		return file.Execute();
	}

	protected override string GetInternalId() => this.fileInfo.FullName;
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
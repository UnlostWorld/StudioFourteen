namespace ScreenshotStudio.Files;

using Microsoft.Win32;
using ScreenshotStudio.Library.Sources;
using ScreenshotStudio.Services;
using ScreenshotStudio.Studio;
using ScreenshotStudio.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WpfUtils.Extensions;

public class FileService : ServiceBase
{
	private static readonly List<FileTypeInfoBase> FileTypeInfos = new()
	{
		new AppearanceFileTypeInfo(),
		new PoseFileTypeInfo(),
		new SceneFileTypeInfo(),
	};

	public DirectoryInfo ScreenshotStudioDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/ScreenshotStudio/");
	public DirectoryInfo BrioDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Brio/");
	public DirectoryInfo AnamnesisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Anamnesis/");
	public DirectoryInfo KtisisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Ktisis/");

	public FileTypeInfoBase? GetTypeInfo(FileInfo? file)
	{
		if (file == null)
			return null;

		foreach (FileTypeInfoBase info in FileTypeInfos)
		{
			if (info.Extension == file.Extension)
			{
				return info;
			}
		}

		return null;
	}

	public FileTypeInfoBase? GetTypeInfo<TFile>()
		where TFile : FileBase
	{
		return this.GetTypeInfo(typeof(TFile));
	}

	public FileTypeInfoBase? GetTypeInfo(FileBase? file)
	{
		if (file == null)
			return null;

		return this.GetTypeInfo(file.GetType());
	}

	public FileTypeInfoBase? GetTypeInfo(Type fileType)
	{
		foreach(FileTypeInfoBase fileTypeInfo in FileTypeInfos)
		{
			if (fileTypeInfo.LoadsType == fileType)
			{
				return fileTypeInfo;
			}
		}

		return null;
	}

	public Task Save(FileBase file, FileInfo fileInfo)
	{
		string json = Serialization.Serializer.Serialize(file);
		File.WriteAllText(fileInfo.FullName, json);
		return Task.CompletedTask;
	}

	public async Task<DirectoryInfo?> ShowDirectoryDialog(DirectoryInfo? defaultInfo = null)
	{
		BackgroundWindow? bgWindow = BackgroundWindow.Instance;
		if (bgWindow == null)
		{
			this.Log.Error("No background window found");
			return null;
		}

		await Threads.UiThread(bgWindow);

		OpenFolderDialog dialog = new OpenFolderDialog();
		dialog.DefaultDirectory = defaultInfo?.FullName.TrimEnd('/', '\\');
		this.PopulateCustomPlaces(dialog);

		bool? result = dialog.ShowDialog(bgWindow);

		if (result != true)
			return null;

		return new DirectoryInfo(dialog.FolderName);
	}

	public void SaveFile(FileBase file)
	{
		this.SaveFileAsync(file).Run();
	}

	public void SaveFile(FileBase file, FileSystemInfo defaultFileInfo)
	{
		this.SaveFileAsync(file, defaultFileInfo).Run();
	}

	public void SaveFile(FileBase file, string defaultFileName)
	{
		this.SaveFileAsync(file, defaultFileName).Run();
	}

	public async Task SaveFileAsync(FileBase file)
	{
		FileTypeInfoBase? fileTypeInfo = this.GetTypeInfo(file);
		string fileName = $"New {fileTypeInfo?.TypeName}";
		await this.SaveFileAsync(file, fileName);
	}

	public async Task SaveFileAsync(FileBase file, string defaultFileName)
	{
		// TODO: lookup last used file for this file type...
		FileTypeInfoBase? fileTypeInfo = this.GetTypeInfo(file);
		FileSystemInfo? defaultFileInfo = new FileInfo($"{this.ScreenshotStudioDir.FullName}/{defaultFileName}{fileTypeInfo?.Extension}");

		await this.SaveFileAsync(file, defaultFileInfo);
	}

	public async Task SaveFileAsync(FileBase file, FileSystemInfo defaultFileInfo)
	{
		// if the file exists, append (2) after the name.
		int count = 2;
		string fileName = Path.GetFileNameWithoutExtension(defaultFileInfo.Name);
		string extension = Path.GetExtension(defaultFileInfo.Extension);
		while (defaultFileInfo.Exists)
		{
			defaultFileInfo = new FileInfo($"{this.ScreenshotStudioDir.FullName}/{fileName} ({count}){extension}");
			count++;
		}

		FileInfo? fileInfo = await this.ShowSaveDialog(defaultFileInfo, file.GetType());

		if (fileInfo == null)
			return;

		await this.Save(file, fileInfo);
	}

	public Task<FileInfo?> ShowSaveDialog<TFile>(FileSystemInfo? defaultInfo = null)
		where TFile : FileBase, new()
	{
		return this.ShowSaveDialog(defaultInfo, typeof(TFile));
	}

	public Task<FileInfo?> ShowSaveDialog(FileSystemInfo? defaultInfo, params Type[] fileType)
	{
		return this.ShowDialog<SaveFileDialog>(defaultInfo, fileType);
	}

	public Task<FileInfo?> ShowOpenDialog<TFile>(FileSystemInfo? defaultInfo = null)
		where TFile : FileBase, new()
	{
		return this.ShowOpenDialog(defaultInfo, typeof(TFile));
	}

	public Task<FileInfo?> ShowOpenDialog(FileSystemInfo? defaultInfo, params Type[] fileType)
	{
		return this.ShowDialog<OpenFileDialog>(defaultInfo, fileType);
	}

	private void PopulateCustomPlaces(CommonItemDialog self)
	{
		foreach (SourceBase src in ServiceManager.Instance.Library.Sources)
		{
			if (src is FileSource fileSource)
			{
				if (fileSource.Directory?.Exists == true)
				{
					FileDialogCustomPlace place = new(fileSource.Directory.FullName);
					self.CustomPlaces.Add(place);
				}
			}
		}
	}

	private Task<FileInfo?> ShowDialog<TDialogType, TFile>(FileSystemInfo? defaultInfo = null)
		where TDialogType : FileDialog, new()
		where TFile : FileBase, new()
	{
		return this.ShowDialog<TDialogType>(defaultInfo, typeof(TFile));
	}

	private Task<FileInfo?> ShowDialog<TDialogType>(FileSystemInfo? defaultInfo, params Type[] fileTypes)
		where TDialogType : FileDialog, new()
	{
		List<FileTypeInfoBase> fileTypeInfos = new();
		foreach(Type fileType in fileTypes)
		{
			FileTypeInfoBase? fileTypeInfo = this.GetTypeInfo(fileType);
			if (fileTypeInfo == null)
				continue;

			fileTypeInfos.Add(fileTypeInfo);
		}

		return this.ShowDialog<TDialogType>(defaultInfo, fileTypeInfos.ToArray());
	}

	private async Task<FileInfo?> ShowDialog<TDialogType>(FileSystemInfo? defaultInfo, params FileTypeInfoBase[] fileTypeInfos)
		where TDialogType : FileDialog, new()
	{
		BackgroundWindow? bgWindow = BackgroundWindow.Instance;
		if (bgWindow == null)
		{
			this.Log.Error("No background window found");
			return null;
		}

		await Threads.UiThread(bgWindow);

		FileDialog dialog = new TDialogType();
		dialog.AddExtension = false;

		StringBuilder filterBuilder = new();

		// All
		if (fileTypeInfos.Length > 1 && dialog is OpenFileDialog)
		{
			filterBuilder.Append("Studio Files|");

			for (int i = 0; i < fileTypeInfos.Length; i++)
			{
				if (i != 0)
					filterBuilder.Append(";");

				filterBuilder.Append($"*{fileTypeInfos[i].Extension}");
			}

			filterBuilder.Append("|");
		}

		// Individual formats
		for (int i = 0; i < fileTypeInfos.Length; i++)
		{
			if (i != 0)
				filterBuilder.Append("|");

			filterBuilder.Append($"{fileTypeInfos[i].TypeName}|*{fileTypeInfos[i].Extension}");
		}

		if (defaultInfo is FileInfo defaultFileInfo)
		{
			dialog.FileName = Path.GetFileNameWithoutExtension(defaultFileInfo.Name);
			dialog.DefaultDirectory = defaultFileInfo.Directory?.FullName.TrimEnd('/', '\\');
		}
		else if (defaultInfo is DirectoryInfo defaultDirectoryInfo)
		{
			dialog.DefaultDirectory = defaultDirectoryInfo.FullName.TrimEnd('/', '\\');
		}

		dialog.Filter = filterBuilder.ToString();
		this.PopulateCustomPlaces(dialog);

		bool? result = dialog.ShowDialog(bgWindow);

		if (result != true)
			return null;

		return new FileInfo(dialog.FileName);
	}
}
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

namespace StudioFourteen.Files;

using Microsoft.Win32;
using StudioFourteen.Appearance;
using StudioFourteen.Environment;
using StudioFourteen.Library.Sources;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

[Service]
public class FileService : ServiceBase
{
	private static readonly List<FileTypeInfoBase> FileTypeInfos = new()
	{
		new AppearanceFileTypeInfo(),
		new PoseFileTypeInfo(),
		new MareFileTypeInfo(),
		new EnvironmentFileTypeInfo(),
	};

	public DirectoryInfo StudioFourteenAppDataDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/StudioFourteen/");
	public DirectoryInfo StudioFourteenDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/StudioFourteen/");
	public DirectoryInfo BrioDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Brio/");
	public DirectoryInfo AnamnesisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Anamnesis/");
	public DirectoryInfo KtisisDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Ktisis/");
	public DirectoryInfo MareDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Mare/");
	public DirectoryInfo AnamnesisStandardDir { get; init; } = new DirectoryInfo($"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/Anamnesis/StandardPoses/");

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
		foreach (FileTypeInfoBase fileTypeInfo in FileTypeInfos)
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
		throw new NotImplementedException();
		////return new DirectoryInfo(dialog.FolderName);
	}

	public void SaveFile(FileBase file)
	{
		this.SaveFileAsync(file).RunAsynchronously();
	}

	public void SaveFile(FileBase file, FileSystemInfo defaultFileInfo)
	{
		this.SaveFileAsync(file, defaultFileInfo).RunAsynchronously();
	}

	public void SaveFile(FileBase file, string defaultFileName)
	{
		this.SaveFileAsync(file, defaultFileName).RunAsynchronously();
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
		FileSystemInfo? defaultFileInfo = new FileInfo($"{this.StudioFourteenDir.FullName}/{defaultFileName}{fileTypeInfo?.Extension}");

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
			defaultFileInfo = new FileInfo($"{this.StudioFourteenDir.FullName}/{fileName} ({count}){extension}");
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
		throw new NotImplementedException();
	}

	public Task<FileInfo?> ShowOpenDialog<TFile>(FileSystemInfo? defaultInfo = null)
		where TFile : FileBase, new()
	{
		return this.ShowOpenDialog(defaultInfo, typeof(TFile));
	}

	public Task<FileInfo?> ShowOpenDialog(FileSystemInfo? defaultInfo, params Type[] fileType)
	{
		throw new NotImplementedException();
	}
}
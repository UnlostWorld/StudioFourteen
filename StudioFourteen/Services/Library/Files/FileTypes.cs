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
using System.Collections.Generic;
using System.IO;

public class FileTypes
{
	private static readonly List<FileTypeInfoBase> FileTypeInfos = new()
	{
		////new AppearanceFileTypeInfo(),
		////new PoseFileTypeInfo(),
		////new MareFileTypeInfo(),
		////new ScriptFileTypeInfo(),
		////new EnvironmentFileTypeInfo(),
	};

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
}
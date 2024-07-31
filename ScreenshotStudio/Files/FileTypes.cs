namespace ScreenshotStudio.Files;
using System.Collections.Generic;
using System.IO;

public static class FileTypes
{
	private static readonly List<FileTypeInfoBase> Infos = new()
	{
		new CharacterFileTypeInfo(),
		new PoseFileTypeInfo(),
	};

	public static FileTypeInfoBase? GetTypeInfo(FileInfo file)
	{
		foreach (FileTypeInfoBase info in Infos)
		{
			if (info.Extension == file.Extension)
			{
				return info;
			}
		}

		return null;
	}
}

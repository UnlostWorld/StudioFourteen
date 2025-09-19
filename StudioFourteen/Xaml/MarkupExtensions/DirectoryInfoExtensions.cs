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

namespace System.IO;

using System;
using System.Collections.Generic;

public static class DirectoryInfoExtensions
{
	private static readonly Dictionary<string, Environment.SpecialFolder> SpecialFolderLookup = new();

	static DirectoryInfoExtensions()
	{
		foreach (Environment.SpecialFolder folder in Enum.GetValues<Environment.SpecialFolder>())
		{
			string path = Environment.GetFolderPath(folder).ToLowerInvariant();

			if (SpecialFolderLookup.ContainsKey(path))
				continue;

			SpecialFolderLookup.Add(path, folder);
		}
	}

	public static bool IsSpecialFolder(this DirectoryInfo self)
	{
		return SpecialFolderLookup.ContainsKey(self.FullName.ToLowerInvariant());
	}

	public static Environment.SpecialFolder? GetSpecialFolder(this DirectoryInfo self)
	{
		Environment.SpecialFolder folder;
		if (!SpecialFolderLookup.TryGetValue(self.FullName.ToLowerInvariant(), out folder))
			return null;

		return folder;
	}

	public static bool IsDirectory(this DirectoryInfo self, DirectoryInfo? other)
	{
		if (other == null)
			return false;

		return Path.GetRelativePath(self.FullName, other.FullName) == ".";
	}
}

// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace FFXIVClientStructs.FFXIV.Client.Game.Object;

using global::System;
using global::System.Runtime.InteropServices;
using global::System.Collections.Generic;

public static class GameObjectExtensions
{
	private static readonly Dictionary<string, string> NameMap = new();

	public static unsafe string? GetNameAsString(ref this GameObject self)
	{
		fixed (byte* ptr = self.Name)
		{
			return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
		}
	}

	public static void SetDisplayName(ref this GameObject self, string displayName)
	{
		NameMap[self.NameString] = displayName;
	}

	public static string GetDisplayName(ref this GameObject self)
	{
		string name = self.NameString;

		if (NameMap.TryGetValue(name, out string? newName))
			return newName;

		return name;
	}
}

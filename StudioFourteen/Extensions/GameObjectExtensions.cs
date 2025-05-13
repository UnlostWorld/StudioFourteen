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

namespace FFXIVClientStructs.FFXIV.Client.Game.Object;

using global::System;
using global::System.Runtime.InteropServices;
using global::System.Collections.Generic;
using StudioFourteen;
using StudioFourteen.Rendering;

public static class GameObjectExtensions
{
	private static readonly Dictionary<string, string> NameMap = new();
	private static readonly Dictionary<int, Color> ColorMap = new();

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

	public static void SetDisplayColor(ref this GameObject self, string displayName)
	{
		NameMap[self.NameString] = displayName;
	}

	public static Color GetDisplayColor(ref this GameObject self)
	{
		Color color;
		if (!ColorMap.TryGetValue(self.ObjectIndex, out color))
		{
			Random r = Random.Shared;
			color = new Color(
				0.5f + r.NextSingle(),
				0.5f + r.NextSingle(),
				0.5f + r.NextSingle(),
				1.0f);

			ColorMap.Add(self.ObjectIndex, color);
		}

		return color;
	}

	public static unsafe Transform GetTransform(ref this GameObject self)
	{
		return Transform.FromTRS(self.DrawObject->Position, self.DrawObject->Rotation, self.DrawObject->Scale);
	}

	public static unsafe void SetTransform(ref this GameObject self, Transform transform)
	{
		if (transform.ToTRS(out var translation, out var rotation, out var scale))
		{
			self.DrawObject->Position = translation;
			self.DrawObject->Rotation = rotation;
			self.DrawObject->Scale = scale;
		}
	}
}

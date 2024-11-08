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

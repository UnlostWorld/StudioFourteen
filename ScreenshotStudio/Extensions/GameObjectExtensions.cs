namespace FFXIVClientStructs.FFXIV.Client.Game.Object;

using global::System;
using global::System.Runtime.InteropServices;

public static class GameObjectExtensions
{
	public static unsafe string? GetNameAsString(ref this GameObject self)
	{
		fixed (byte* ptr = self.Name)
		{
			return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
		}
	}
}

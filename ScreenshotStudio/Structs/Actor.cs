// Ktisis
// https://github.com/ktisis-tools/Ktisis/
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

// Anamnesis
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs
// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorBasicMemory.cs
namespace ScreenshotStudio.Structs;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Utilities;

[StructLayout(LayoutKind.Explicit, Size = 0x84A)]
public struct Actor
{
	[FieldOffset(0)] public GameObject GameObject;
	[FieldOffset(0x88)] public byte ObjectID;
	[FieldOffset(0x100)] public unsafe ActorModel* Model;
	[FieldOffset(0x118)] public RenderMode RenderMode;
	[FieldOffset(0x1AC)] public uint ModelId;
	[FieldOffset(0x708)] public ActorDrawData DrawData;
	[FieldOffset(0x89E)] public bool IsHatHidden;

	public unsafe string? Name
	{
		get
		{
			fixed (byte* ptr = this.GameObject.Name)
			{
				return ptr == null ? null : Marshal.PtrToStringUTF8((IntPtr)ptr);
			}
		}
	}

	public byte GetCustomizeValue(CustomizeIndex option)
	{
		return this.DrawData.Customize.GetValue(option);
	}

	public bool SetCustomizeValue(CustomizeIndex option, byte value, bool apply = true)
	{
		this.DrawData.Customize.SetValue(option, value);

		bool needsRedraw = option == CustomizeIndex.Race;
		needsRedraw |= option == CustomizeIndex.Tribe;
		needsRedraw |= option == CustomizeIndex.ModelType;
		needsRedraw |= option == CustomizeIndex.Gender;

		if (apply)
		{
			this.UpdateCustomize(needsRedraw);
		}

		return needsRedraw;
	}

	public unsafe void UpdateCustomize(bool redraw)
	{
		Threads.RunOnFrameworkThread(this, (p) => ((Actor*)p)->UpdateCustomizeInternal(redraw));
	}

	private unsafe void UpdateCustomizeInternal(bool redraw)
	{
		Threads.VerifyFrameworkThread();

		if (!redraw)
		{
			fixed (Customize* custom = &this.DrawData.Customize)
			{
				bool success = ((Human*)this.Model)->UpdateDrawData((byte*)custom, true);
				if (!success)
				{
					Logging.Shared.Warning("Updating Draw Data failed. (should this have been a redraw?)");
					redraw = true;
				}
			}
		}

		if (redraw)
		{
			this.GameObject.DisableDraw();
			this.GameObject.EnableDraw();
		}
	}
}

public enum RenderMode : uint
{
	Draw = 0,
	Unload = 2,
	Load = 4,
}

[Flags]
public enum ActorFlags : byte
{
	None = 0,
	WeaponsVisible = 1,
	WeaponsDrawn = 2,
	VisorToggle = 8,
}
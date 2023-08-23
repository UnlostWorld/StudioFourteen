// © XivTools.
// Licensed under the MIT license.

//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Actor.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorDrawData.cs

//// Anamnesis
//// https://github.com/imchillin/Anamnesis/blob/master/Anamnesis/Memory/ActorCustomizeMemory.cs

namespace ScreenshotStudio.Structs;

using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

public unsafe class ActorViewModel : StructViewModelBase<Actor>
{
	public ActorViewModel(IntPtr ptr)
	{
		this.SetAddress(ptr);
	}

	public string DisplayName => this.Struct?.Name ?? "Unknown Actor";

	public ActorDrawDataViewModel DrawData { get; init; } = new();
	public ActorModelViewModel Model { get; init; } = new();

	public override void Tick()
	{
		base.Tick();
	}
}

[StructLayout(LayoutKind.Explicit, Size = 0x84A)]
public struct Actor
{
	[FieldOffset(0)] public GameObject GameObject;
	[FieldOffset(0x88)] public byte ObjectID;
	[FieldOffset(0x100)] public unsafe ActorModel* Model;
	[FieldOffset(0x114)] public RenderMode RenderMode;
	[FieldOffset(0x1B4)] public uint ModelId;
	[FieldOffset(0x6E8)] public ActorDrawData DrawData;
	[FieldOffset(0x876)] public bool IsHatHidden;

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
//// Ktisis
//// https://github.com/ktisis-tools/Ktisis/
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/ActorModel.cs
//// https://github.com/ktisis-tools/Ktisis/blob/main/Ktisis/Structs/Actor/Attach.cs

namespace ScreenshotStudio.Structs;

using System.Numerics;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok;

[StructLayout(LayoutKind.Explicit)]
public struct ActorModel
{
	[FieldOffset(0)] public Object Object;
	[FieldOffset(0x050)] public hkQsTransformf Transform;
	[FieldOffset(0x050)] public Vector3 Position;
	[FieldOffset(0x060)] public Quaternion Rotation;
	[FieldOffset(0x070)] public Vector3 Scale;
	[FieldOffset(0x88)] public byte Flags;
	[FieldOffset(0x0A0)] public unsafe Skeleton* Skeleton;
	[FieldOffset(0x0D0)] public Attach Attach;
	[FieldOffset(0x148)] public unsafe Bust* Bust;
	[FieldOffset(0x274)] public float Height;
	[FieldOffset(0x370)] public nint Sklb;
}

[StructLayout(LayoutKind.Explicit)]
public struct Bust
{
	[FieldOffset(0x50)] public ushort Left;
	[FieldOffset(0x52)] public ushort Right;
	[FieldOffset(0x68)] public Vector3 Scale;
}

[StructLayout(LayoutKind.Explicit, Size = 0x78)]
public struct Attach
{
	[FieldOffset(0x50)] public uint Type; // 1-5
	[FieldOffset(0x68)] public uint Count;
	[FieldOffset(0x70)] public unsafe BoneAttach* BoneAttach;
	[FieldOffset(0x58)] public unsafe Skeleton* TargetSkeleton;
	[FieldOffset(0x60)] public unsafe Skeleton* ParentSkeleton;
}

[StructLayout(LayoutKind.Explicit, Size = 0x68)]
public struct BoneAttach
{
	[FieldOffset(0x02)] public ushort BoneId;
	[FieldOffset(0x30)] public float Scale;
}
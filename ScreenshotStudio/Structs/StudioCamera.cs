// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Structs;

using FFXIVClientStructs.Havok;
using ScreenshotStudio.Structs.Extensions;
using System.Runtime.InteropServices;
using XivToolsWpf.Meida3D;

[StructLayout(LayoutKind.Explicit, Size = 688)]
public struct StudioCamera // : FFXIVClientStructs.FFXIV.Client.Game.Camera
{
	[FieldOffset(276)] public float Distance;
	[FieldOffset(280)] public float MinDistance;
	[FieldOffset(284)] public float MaxDistance;
	[FieldOffset(288)] public float FoV;
	[FieldOffset(292)] public float MinFoV;
	[FieldOffset(296)] public float MaxFoV;

	[FieldOffset(304)] public float AngleX;
	[FieldOffset(308)] public float AngleY;
	[FieldOffset(332)] public float YMin;
	[FieldOffset(336)] public float YMax;
	[FieldOffset(336)] public float PanX;
	[FieldOffset(340)] public float PanY;
	[FieldOffset(352)] public float Roll;

	public hkVector4f EulerRotation
	{
		get
		{
			hkVector4f v = default;
			v.Y = (float)MathUtils.RadiansToDegrees((double)this.AngleX) - 180;
			v.Z = (float)-MathUtils.RadiansToDegrees((double)this.AngleY);
			v.X = (float)MathUtils.RadiansToDegrees((double)this.Roll);
			return v;
		}
	}

	public hkQuaternionf Rotation => HkQuaternionExtensions.FromEuler(this.EulerRotation);
}

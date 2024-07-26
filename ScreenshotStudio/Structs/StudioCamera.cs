namespace ScreenshotStudio.Structs;

using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using ScreenshotStudio.Structs.Extensions;
using System.Runtime.InteropServices;

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
			v.Y = (this.AngleX * HkQuaternionExtensions.Rad2Deg) - 180;
			v.Z = -(this.AngleY * HkQuaternionExtensions.Rad2Deg);
			v.X = this.Roll * HkQuaternionExtensions.Rad2Deg;
			return v;
		}
	}

	public hkQuaternionf Rotation => HkQuaternionExtensions.FromEuler(this.EulerRotation);
}

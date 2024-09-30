namespace FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;

using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;

public static class HkTransformExtensions
{
	public static void Add(this ref hkQsTransformf self, hkQsTransformf other)
	{
		self.Translation.Add(other.Translation);
		self.Rotation.Multiply(other.Rotation);
		self.Scale.Add(other.Scale);
	}

	public static void Subtract(this ref hkQsTransformf self, hkQsTransformf other)
	{
		self.Translation.Subtract(other.Translation);
		self.Rotation.Divide(other.Rotation);
		self.Scale.Subtract(other.Scale);
	}
}

namespace System.Numerics;

public static class Vector3Extensions
{
	public static bool IsApproximately(this Vector3 a, Vector3 b, float delta = float.Epsilon)
	{
		return a.X.IsApproximately(b.X, delta)
			&& a.Y.IsApproximately(b.Y, delta)
			&& a.Z.IsApproximately(b.Z, delta);
	}
}

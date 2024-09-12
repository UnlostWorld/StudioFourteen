namespace System;

public static class FloatExtensions
{
	public static bool IsApproximately(this float a, float b, float delta = float.Epsilon)
	{
		float dX = Math.Abs(a - b);
		return dX <= delta;
	}
}

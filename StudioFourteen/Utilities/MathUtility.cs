namespace StudioFourteen.Utilities;

using System.Numerics;

public static class MathUtility
{
	public static float Wrap(float angle)
	{
		while (angle > 180)
			angle -= 360;

		while (angle < -180)
			angle += 360;

		return angle;
	}

	public static Vector2 Wrap(Vector2 value)
	{
		return new Vector2(Wrap(value.X), Wrap(value.Y));
	}
}

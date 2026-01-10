// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Services.Numerics;

using System.Numerics;

public static class Math
{
	public static readonly float Deg2Rad = (Math.PI * 2) / 360;
	public static readonly float Rad2Deg = 360 / (Math.PI * 2);

	public static float PI => System.MathF.PI;

	public static float Sin(float x) => System.MathF.Sin(x);
	public static float Cos(float x) => System.MathF.Cos(x);
	public static float Tan(float x) => System.MathF.Tan(x);

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

	public static float InverseLerp(float a, float b, float v)
	{
		return (v - a) / (b - a);
	}

	// https://stackoverflow.com/a/51906100/9934501
	public static Vector2 FindNearestPointOnLine(Vector2 origin, Vector2 end, Vector2 point)
	{
		// Get heading
		Vector2 heading = end - origin;
		float magnitudeMax = heading.Length();
		heading = Vector2.Normalize(heading);

		// Do projection from the point but clamp it
		Vector2 lhs = point - origin;
		float dotP = Vector2.Dot(lhs, heading);
		dotP = System.Math.Clamp(dotP, 0f, magnitudeMax);
		return origin + (heading * dotP);
	}
}
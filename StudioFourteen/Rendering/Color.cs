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

namespace StudioFourteen.Rendering;

using System.Numerics;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public struct Color
{
	public static readonly Color White = new(1.0f, 1.0f, 1.0f, 1.0f);
	public static readonly Color Black = new(0.0f, 0.0f, 0.0f, 1.0f);
	public static readonly Color Transparent = new(0.0f, 0.0f, 0.0f, 0.0f);

	public float R;
	public float G;
	public float B;
	public float A;

	public Color(float r, float g, float b, float a)
	{
		this.R = r;
		this.G = g;
		this.B = b;
		this.A = a;
	}

	public Color(byte a, byte r, byte g, byte b)
	{
		this.R = r / 255.0f;
		this.G = g / 255.0f;
		this.B = b / 255.0f;
		this.A = a / 255.0f;
	}

	public static implicit operator Vector4(Color color)
	{
		return new(color.R, color.G, color.B, color.A);
	}

	public static Color Lerp(Color from, Color to, float p)
	{
		return new Color(
			float.Lerp(from.R, to.R, p),
			float.Lerp(from.G, to.G, p),
			float.Lerp(from.B, to.B, p),
			float.Lerp(from.A, to.A, p));
	}
}
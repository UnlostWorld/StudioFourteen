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

namespace SixLabors.ImageSharp.PixelFormats;

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public partial struct GreyscaleFloat : IPixel<GreyscaleFloat>, IPackedVector<float>
{
	public float Value;

	public GreyscaleFloat(float v)
	{
		this.Value = v;
	}

	public float PackedValue
	{
		readonly get => this.Value;
		set => this.Value = value;
	}

	public static implicit operator Color(GreyscaleFloat source) => new(new Vector4(source.Value, source.Value, source.Value, 1.0f));
	public static implicit operator GreyscaleFloat(Color color) => new(((Vector4)color).X);

	public static bool operator ==(GreyscaleFloat left, GreyscaleFloat right) => left.Equals(right);
	public static bool operator !=(GreyscaleFloat left, GreyscaleFloat right) => !left.Equals(right);

	public readonly PixelOperations<GreyscaleFloat> CreatePixelOperations() => new PixelOperations<GreyscaleFloat>();

	public void FromScaledVector4(Vector4 vector) => this.FromVector4(vector);
	public readonly Vector4 ToScaledVector4() => this.ToVector4();
	public void FromVector4(Vector4 vector) => this.Pack(vector);
	public readonly Vector4 ToVector4() => new Vector4(this.Value, this.Value, this.Value, this.Value);

	public override readonly bool Equals(object? obj) => obj is GreyscaleFloat other && this.Equals(other);
	public readonly bool Equals(GreyscaleFloat other) => this.PackedValue.Equals(other.PackedValue);
	public override readonly int GetHashCode() => this.PackedValue.GetHashCode();

	public override readonly string ToString() => $"GreyscaleFloat({this.Value})";

	// No support for conversion. sorry!
	public void FromArgb32(Argb32 source) => throw new System.NotImplementedException();
	public void FromBgra5551(Bgra5551 source) => throw new System.NotImplementedException();
	public void FromBgr24(Bgr24 source) => throw new System.NotImplementedException();
	public void FromBgra32(Bgra32 source) => throw new System.NotImplementedException();
	public void FromAbgr32(Abgr32 source) => throw new System.NotImplementedException();
	public void FromL8(L8 source) => throw new System.NotImplementedException();
	public void FromL16(L16 source) => throw new System.NotImplementedException();
	public void FromLa16(La16 source) => throw new System.NotImplementedException();
	public void FromLa32(La32 source) => throw new System.NotImplementedException();
	public void FromRgb24(Rgb24 source) => throw new System.NotImplementedException();
	public void FromRgba32(Rgba32 source) => throw new System.NotImplementedException();
	public void FromRgb48(Rgb48 source) => throw new System.NotImplementedException();
	public void FromRgba64(Rgba64 source) => throw new System.NotImplementedException();

	public void ToRgba32(ref Rgba32 dest)
	{
		byte bValue = (byte)(this.Value * 255);

		dest.A = 255;
		dest.R = bValue;
		dest.G = bValue;
		dest.B = bValue;
	}

	private void Pack(Vector4 vector)
	{
		this.Value = vector.X;
	}
}

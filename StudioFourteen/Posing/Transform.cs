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

namespace StudioFourteen.Posing;

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;
using System;
using System.Numerics;

public struct Transform : IEquatable<Transform>
{
	public Vector3 Translation = Vector3.Zero;
	public Quaternion Rotation = Quaternion.Identity;
	public Vector3 Scale = Vector3.One;

	public Transform()
	{
		this.Translation = Vector3.Zero;
		this.Rotation = Quaternion.Identity;
		this.Scale = Vector3.One;
	}

	public static implicit operator Transform(hkQsTransformf transform)
	{
		Transform t = default;
		t.Translation = transform.Translation.ToVector3();
		t.Rotation = transform.Rotation.ToQuaternion();
		t.Scale = transform.Scale.ToVector3();
		return t;
	}

	public static implicit operator hkQsTransformf(Transform transform)
	{
		hkQsTransformf t = default;
		t.Translation = transform.Translation.ToHkVector();
		t.Rotation = transform.Rotation.ToHkQuaternion();
		t.Scale = transform.Scale.ToHkVector();
		return t;
	}

	public static Transform operator *(Transform left, Transform right)
	{
		Transform t = default;
		t.Translation = left.Translation + right.Translation;
		t.Rotation = Quaternion.Normalize(left.Rotation * right.Rotation);
		t.Scale = left.Scale * right.Scale;
		return t;
	}

	public static Transform operator /(Transform left, Transform right)
	{
		Transform t = default;
		t.Translation = left.Translation - right.Translation;
		t.Rotation = Quaternion.Normalize(left.Rotation / right.Rotation);
		t.Scale = left.Scale / right.Scale;
		return t;
	}

	public static Transform operator -(Transform left, Transform right)
	{
		Transform t = default;
		t.Translation = left.Translation - right.Translation;
		t.Rotation = Quaternion.Normalize(Quaternion.Inverse(left.Rotation) * right.Rotation);
		t.Scale = left.Scale / right.Scale;
		return t;
	}

	public static bool operator !=(Transform left, Transform right)
	{
		return !(left == right);
	}

	public static bool operator ==(Transform left, Transform right)
	{
		return left.Translation == right.Translation
			&& left.Rotation == right.Rotation
			&& left.Scale == right.Scale;
	}

	public static Transform Lerp(Transform from, Transform to, float amount)
	{
		Transform t = default;
		t.Translation = Vector3.Lerp(from.Translation, to.Translation, amount);
		t.Rotation = Quaternion.Lerp(from.Rotation, to.Rotation, amount);
		t.Scale = Vector3.Lerp(from.Scale, to.Scale, amount);
		return t;
	}

	public override readonly bool Equals(object? obj)
	{
		return obj is Transform transform && this.Equals(transform);
	}

	public readonly bool Equals(Transform other)
	{
		return this.Translation.Equals(other.Translation) &&
			   this.Rotation.Equals(other.Rotation) &&
			   this.Scale.Equals(other.Scale);
	}

	public override readonly int GetHashCode()
	{
		return HashCode.Combine(this.Translation, this.Rotation, this.Scale);
	}
}
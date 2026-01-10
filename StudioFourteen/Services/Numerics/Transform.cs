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

using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using System;
using System.Numerics;

public struct Transform : IEquatable<Transform>
{
	private Matrix4x4 matrix = Matrix4x4.Identity;

	public Transform()
	{
	}

	public Transform(Matrix4x4 matrix)
	{
		this.matrix = matrix;
	}

	public static Transform Identity => new(Matrix4x4.Identity);

	public Vector3 Translation
	{
		get => this.matrix.Translation;
		set => this.matrix.Translation = value;
	}

	public Quaternion Rotation
	{
		get
		{
			if (!Matrix4x4.Decompose(this.matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
				throw new Exception("Failed to unpack matrix4x4");
			return rotation;
		}
	}

	public Vector3 Scale
	{
		get
		{
			if (!Matrix4x4.Decompose(this.matrix, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
				throw new Exception("Failed to unpack matrix4x4");

			return scale;
		}
	}

	public static implicit operator Transform(Matrix4x4 matrix) => new Transform(matrix);

	public static implicit operator Transform(hkQsTransformf transform)
	{
		return FromTRS(
			transform.Translation.ToVector3(),
			transform.Rotation.ToQuaternion(),
			transform.Scale.ToVector3());
	}

	public static implicit operator hkQsTransformf(Transform transform)
	{
		hkQsTransformf t = default;
		t.Translation = transform.Translation.ToHkVector();
		t.Rotation = transform.Rotation.ToHkQuaternion();
		t.Scale = transform.Scale.ToHkVector();
		return t;
	}

	public static Transform operator +(Transform left, Transform right) => left.matrix + right.matrix;
	public static Transform operator *(Transform left, Transform right) => left.matrix * right.matrix;
	public static Transform operator -(Transform left, Transform right) => left.matrix - right.matrix;

	public static bool operator !=(Transform left, Transform right) => !(left == right);
	public static bool operator ==(Transform left, Transform right) => left.matrix == right.matrix;

	public static Transform FromTRS(Vector3 translation, Quaternion rotation, Vector3 scale)
	{
		Matrix4x4 mat = Matrix4x4.Identity;
		mat *= Matrix4x4.CreateScale(scale);
		mat *= Matrix4x4.CreateFromQuaternion(rotation);
		mat *= Matrix4x4.CreateTranslation(translation);
		return mat;
	}

	public static Transform FromTranslation(Vector3 translation) => Matrix4x4.CreateTranslation(translation);
	public static Transform FromTranslation(float x, float y, float z) => Matrix4x4.CreateTranslation(x, y, z);
	public static Transform FromRotation(Quaternion rotation) => Matrix4x4.CreateFromQuaternion(rotation);

	public static Transform FromRotation(float x, float y, float z) =>
		Matrix4x4.CreateFromYawPitchRoll(
			Math.Deg2Rad * x,
			Math.Deg2Rad * y,
			Math.Deg2Rad * z);

	public static Transform FromScale(Vector3 scale) => Matrix4x4.CreateScale(scale);
	public static Transform FromScale(float x, float y, float z) => Matrix4x4.CreateScale(x, y, z);
	public static Transform FromScale(float scale) => Matrix4x4.CreateScale(scale);

	public static Transform Lerp(Transform from, Transform to, float amount)
	{
		return Matrix4x4.Lerp(from.matrix, to.matrix, amount);
	}

	public static bool Divide(Transform left, Transform right, out Transform? result)
	{
		result = null;

		Matrix4x4 r;
		if (!Matrix4x4.Invert(right.ToMatrix(), out r))
			return false;

		result = left.ToMatrix() * r;
		return true;
	}

	public Matrix4x4 ToMatrix() => this.matrix;
	public override readonly bool Equals(object? obj) => this.matrix.Equals(obj);
	public readonly bool Equals(Transform other) => this.matrix == other.matrix;
	public override readonly int GetHashCode() => this.matrix.GetHashCode();

	public bool ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale)
	{
		bool success = Matrix4x4.Decompose(this.matrix, out scale, out rotation, out translation);
		if (!success)
			return false;

		rotation = Quaternion.Normalize(rotation);
		return true;
	}

	public bool DivideBy(Transform right, out Transform? result)
	{
		return Transform.Divide(this, right, out result);
	}

	public Vector4 TransformViewProjection(Vector4 worldPos)
	{
		Vector4 vector = Vector4.Transform(worldPos, this.matrix);

		vector *= MathF.Abs(1f / vector.W);
		return new Vector4
		{
			X = (vector.X + 1f) * 0.5f,
			Y = (1f - vector.Y) * 0.5f,
			Z = vector.Z,
			W = 1,
		};
	}
}
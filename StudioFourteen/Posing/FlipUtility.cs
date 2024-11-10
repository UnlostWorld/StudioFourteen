namespace StudioFourteen.Posing;

using System.Numerics;

public static class FlipUtility
{
	public static Transform Flip(Transform transform, MirrorModes mirrorMode = MirrorModes.MirrorTRCopyS)
	{
		Transform mirrorTransform = new();

		Quaternion mirrorRotation = transform.Rotation;
		if (mirrorMode == MirrorModes.MirrorTRCopyS)
		{
			mirrorRotation.W = transform.Rotation.W;
			mirrorRotation.X = -transform.Rotation.X;
			mirrorRotation.Y = -transform.Rotation.Y;
			mirrorRotation.Z = transform.Rotation.Z;
		}

		mirrorTransform.Rotation = mirrorRotation;
		mirrorTransform.Scale = transform.Scale;

		mirrorTransform.Translation = new(
			transform.Translation.X,
			transform.Translation.Y,
			-transform.Translation.Z);

		return mirrorTransform;
	}

	public static BoneTransform Flip(BoneTransform boneTransform, MirrorModes mirrorMode = MirrorModes.MirrorTRCopyS)
	{
		BoneTransform mirrorTransform = new();

		if (boneTransform.Rotation != null)
		{
			Quaternion mirrorRotation = (Quaternion)boneTransform.Rotation;
			if (mirrorMode == MirrorModes.MirrorTRCopyS)
			{
				mirrorRotation.W = boneTransform.Rotation.Value.W;
				mirrorRotation.X = -boneTransform.Rotation.Value.X;
				mirrorRotation.Y = -boneTransform.Rotation.Value.Y;
				mirrorRotation.Z = boneTransform.Rotation.Value.Z;
			}

			mirrorTransform.Rotation = mirrorRotation;

			if (boneTransform.Scale != null)
			{
				mirrorTransform.Scale = boneTransform.Scale.Value;
			}

			if (boneTransform.Translation != null)
			{
				mirrorTransform.Translation = new(
					boneTransform.Translation.Value.X,
					boneTransform.Translation.Value.Y,
					-boneTransform.Translation.Value.Z);
			}
		}

		return mirrorTransform;
	}
}

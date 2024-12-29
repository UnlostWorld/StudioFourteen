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

using System;
using System.Numerics;

public static class FlipUtility
{
	public static Transform Flip(Transform transform, MirrorModes mirrorMode = MirrorModes.MirrorTRCopyS)
	{
		Transform mirrorTransform = new();

		if (transform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
		{
			Vector3 mirrorTranslation = new(
				transform.Translation.X,
				transform.Translation.Y,
				-transform.Translation.Z);

			Quaternion mirrorRotation = rotation;
			if (mirrorMode == MirrorModes.MirrorTRCopyS)
			{
				mirrorRotation.W = rotation.W;
				mirrorRotation.X = -rotation.X;
				mirrorRotation.Y = -rotation.Y;
				mirrorRotation.Z = rotation.Z;
			}

			Vector3 mirrorScale = scale;

			return Transform.FromTRS(mirrorTranslation, mirrorRotation, mirrorScale);
		}

		return transform;
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

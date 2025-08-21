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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using System.Numerics;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;

public class LoadModelSpaceTransform(Transform transform, bool blend = false)
	: LocalSpaceTransformOperationBase(blend)
{
	public override unsafe Transform? Apply(BoneReference bone, hkaPose* pPose, short boneIndex)
	{
		hkQsTransformf* boneModelTransform = pPose->AccessBoneModelSpace(boneIndex, hkaPose.PropagateOrNot.Propagate);

		if (Matrix4x4.Decompose(
			transform.ToMatrix(),
			out Vector3 scale,
			out Quaternion rotation,
			out Vector3 translation))
		{
			translation = Vector3.Clamp(translation, BoneReference.MinTranslate, BoneReference.MaxTranslate);
			boneModelTransform->Translation.Set(translation);

			boneModelTransform->Rotation.Set(rotation.ToHkQuaternion());

			scale = Vector3.Clamp(scale, BoneReference.MinScale, BoneReference.MaxScale);
			boneModelTransform->Scale.Set(scale);
		}

		hkQsTransformf hkTransform = *pPose->AccessBoneLocalSpace(boneIndex);
		return (Transform)hkTransform;
	}
}

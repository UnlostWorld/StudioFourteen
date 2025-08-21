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
using StudioFourteen.Posing;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;

public class LoadModelSpaceBoneTransform(BoneTransform boneTransform, bool blend = false)
	: LocalSpaceTransformOperationBase(blend)
{
	public override unsafe Transform? Apply(BoneReference bone, hkaPose* pPose, short boneIndex)
	{
		hkQsTransformf* boneModelTransform = pPose->AccessBoneModelSpace(boneIndex, hkaPose.PropagateOrNot.Propagate);

		if (boneTransform.Translation != null)
			boneModelTransform->Translation.Set((Vector3)boneTransform.Translation);

		if (boneTransform.Rotation != null)
			boneModelTransform->Rotation.Set((Quaternion)boneTransform.Rotation);

		if (boneTransform.Scale != null)
			boneModelTransform->Scale.Set((Vector3)boneTransform.Scale);

		hkQsTransformf transform = *pPose->AccessBoneLocalSpace(boneIndex);
		return (Transform)transform;
	}
}

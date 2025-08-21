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

namespace StudioFourteen.Scene.Characters.Skeletons;

using System.Collections.Generic;
using System.Numerics;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using StudioFourteen;
using StudioFourteen.Posing;
using StudioFourteen.Scene.GameObjects.Characters.Skeletons;
using StudioFourteen.Structs;
using StudioFourteen.Structs.Extensions;

public class FlipBoneOperation() : LocalSpaceTransformOperationBase(false)
{
	private readonly HashSet<string> processedBones = new();

	public override unsafe Transform? Apply(BoneReference bone, hkaPose* pPose, short boneIndex)
	{
		if (this.processedBones.Contains(bone.BoneName))
			return null;

		this.processedBones.Add(bone.BoneName);

		if (bone.Mirror != null)
		{
			if (bone.BoneName.EndsWith("_r"))
				return null;

			Transform? a = bone.LocalSpaceTransform;
			if (a == null)
				a = Transform.Identity;

			Transform? b = bone.Mirror.LocalSpaceTransform;
			if (b == null)
				b = Transform.Identity;

			a = FlipUtility.Flip(a.Value, MirrorModes.MirrorTRCopyS);
			b = FlipUtility.Flip(b.Value, MirrorModes.MirrorTRCopyS);

			bone.Perform(new LoadLocalSpaceTransformOperation(b.Value, false));
			bone.Mirror.Perform(new LoadLocalSpaceTransformOperation(a.Value, false));
			return null;
		}
		else
		{
			if (bone.ModelSpaceTransform == null)
				return null;

			hkQsTransformf hkOldLocalTransform = *pPose->AccessBoneLocalSpace(boneIndex);

			Transform curentModelTransform = (Transform)bone.ModelSpaceTransform;

			if (curentModelTransform.ToTRS(out Vector3 translation, out Quaternion rotation, out Vector3 scale))
			{
				////rotation = new Quaternion(rotation.Z, rotation.W, rotation.X, rotation.Y);
				Vector3 e = rotation.ToEuler();
				e.X = 90 + (e.X - 90);
				e.Y = -e.Y;
				////e.Z = -e.Z;

				rotation.FromEuler(e);

				hkQsTransformf* boneModelTransform = pPose->AccessBoneModelSpace(boneIndex, hkaPose.PropagateOrNot.Propagate);
				boneModelTransform->Rotation.Set(rotation.ToHkQuaternion());
			}

			hkQsTransformf* pNewLocalTransform = pPose->AccessBoneLocalSpace(boneIndex);
			Transform newLocalTransform = *pNewLocalTransform;
			newLocalTransform.Translation = hkOldLocalTransform.Translation.ToVector3();

			// Restore the original local transform
			pNewLocalTransform->Rotation = hkOldLocalTransform.Rotation;
			pNewLocalTransform->Scale = hkOldLocalTransform.Scale;
			pNewLocalTransform->Translation = hkOldLocalTransform.Translation;

			return newLocalTransform;
		}
	}
}
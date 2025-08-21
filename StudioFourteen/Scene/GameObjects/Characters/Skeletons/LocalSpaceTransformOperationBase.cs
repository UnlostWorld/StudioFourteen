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

using FFXIVClientStructs.Havok.Animation.Rig;

public abstract class LocalSpaceTransformOperationBase(bool blend = false) : BoneOperationBase(blend)
{
	public sealed override unsafe Transform? Apply(BoneReference bone, Transform baseLocalTransform, hkaPose* pPose, short boneIndex)
	{
		Transform? localSpaceTransform = this.Apply(bone, pPose, boneIndex);

		if (localSpaceTransform == null)
			return null;

		localSpaceTransform.Value.DivideBy(baseLocalTransform, out Transform? toTransform);
		return toTransform;
	}

	public unsafe abstract Transform? Apply(BoneReference bone, hkaPose* pPose, short boneIndex);
}

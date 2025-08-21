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

using System;
using System.Numerics;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Posing;

public class LoadReferenceRelativeTransform
	: LocalSpaceTransformOperationBase
{
	private Transform transform;

	public LoadReferenceRelativeTransform(Transform transform, bool blend = false)
		: base(blend)
	{
		this.transform = transform;
	}

	public LoadReferenceRelativeTransform(BoneTransform transform, bool blend = false)
		: base(blend)
	{
		this.transform = Transform.FromTRS(
			transform.Translation ?? Vector3.Zero,
			transform.Rotation ?? Quaternion.Identity,
			transform.Scale ?? Vector3.One);
	}

	public override unsafe Transform? Apply(BoneReference bone, hkaPose* pPose, short boneIndex)
	{
		if (bone.ReferenceTransform == null)
			throw new Exception("Bone has no reference transform");

		return this.transform * (Transform)bone.ReferenceTransform;
	}
}

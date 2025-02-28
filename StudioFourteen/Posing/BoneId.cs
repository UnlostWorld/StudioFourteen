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

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Plugin;
using StudioFourteen.Utilities;
using System;

public readonly struct BoneId(int objectTableIndex, int partialSkeletonIndex, byte poseIndex, short boneIndex)
	: IEquatable<BoneId>, IComparable<BoneId>
{
	public readonly int ObjectTableIndex = objectTableIndex;
	public readonly int PartialSkeletonIndex = partialSkeletonIndex;
	public readonly byte PoseIndex = poseIndex;
	public readonly short BoneIndex = boneIndex;

	public static bool operator ==(BoneId left, BoneId right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(BoneId left, BoneId right)
	{
		return !(left == right);
	}

	public override bool Equals(object? obj)
	{
		return obj is BoneId id && this.Equals(id);
	}

	public bool Equals(BoneId other)
	{
		return this.ObjectTableIndex == other.ObjectTableIndex &&
			this.PartialSkeletonIndex == other.PartialSkeletonIndex &&
			this.PoseIndex == other.PoseIndex &&
			this.BoneIndex == other.BoneIndex;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(this.ObjectTableIndex, this.PartialSkeletonIndex, this.PoseIndex, this.BoneIndex);
	}

	public override string ToString()
	{
		return $"Bone {this.ObjectTableIndex} {this.PartialSkeletonIndex} {this.PoseIndex} {this.BoneIndex}";
	}

	public int CompareTo(BoneId other)
	{
		int val = this.ObjectTableIndex.CompareTo(other.ObjectTableIndex);
		if (val == 0)
			val = this.PartialSkeletonIndex.CompareTo(other.PartialSkeletonIndex);

		if (val == 0)
			val = this.PoseIndex.CompareTo(other.PoseIndex);

		if (val == 0)
			val = this.BoneIndex.CompareTo(other.BoneIndex);

		return val;
	}

	public unsafe bool Resolve(out Character* character, out Skeleton* skeleton, out PartialSkeleton* partialSkeleton, out hkaPose* pose)
	{
		character = null;
		skeleton = null;
		partialSkeleton = null;
		pose = null;

		if (DalamudServices.ObjectTable == null)
			return false;

		Threads.VerifyFrameworkThread();

		character = (Character*)DalamudServices.ObjectTable.GetObjectAddress(this.ObjectTableIndex);
		if (character == null)
			return false;

		if (!character->CanDraw())
			return false;

		CharacterBase* characterBase = character->GetCharacterBase();
		if (characterBase == null)
			return false;

		skeleton = characterBase->Skeleton;
		if (skeleton == null)
			return false;

		partialSkeleton = &skeleton->PartialSkeletons[this.PartialSkeletonIndex];
		if (partialSkeleton == null)
			return false;

		pose = partialSkeleton->GetHavokPose(this.PoseIndex);
		if (pose == null || pose->Skeleton == null)
			return false;

		return true;
	}
}

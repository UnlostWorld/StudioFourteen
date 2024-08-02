namespace ScreenshotStudio.Posing;

using System;

public readonly struct BoneId(int objectTableIndex, int partialSkeletonIndex, byte poseIndex, short boneIndex, string? name = null)
	: IEquatable<BoneId>, IComparable<BoneId>
{
	public readonly int ObjectTableIndex = objectTableIndex;
	public readonly int PartialSkeletonIndex = partialSkeletonIndex;
	public readonly byte PoseIndex = poseIndex;
	public readonly short BoneIndex = boneIndex;
	public readonly string? BoneName = name;

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
}

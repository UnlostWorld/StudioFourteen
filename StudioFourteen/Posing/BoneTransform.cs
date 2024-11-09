// Brio
// https://github.com/Etheirys/Brio/tree/main/Brio/Game/Posing/SkeletonService.cs

namespace StudioFourteen.Posing;
using Newtonsoft.Json;
using System.Numerics;

public class BoneTransform
{
	public BoneTransform()
	{
	}

	public BoneTransform(BoneTransform other)
	{
		this.Translation = other.Translation;
		this.Rotation = other.Rotation;
		this.Scale = other.Scale;
	}

	[JsonProperty("T")]
	public Vector3? Translation { get; set; }

	[JsonProperty("R")]
	public Quaternion? Rotation { get; set; }

	[JsonProperty("S")]
	public Vector3? Scale { get; set; }
}
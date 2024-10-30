namespace StudioFourteen.Overlays;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System.Numerics;

public class BoundingBoxOverlay : BoxOverlay
{
	public unsafe void Update(Character* pCharacter)
	{
		Vector3 position = pCharacter->DrawObject->Position;
		position.Y += pCharacter->Height;
		this.Translation = position;

		this.Rotation = pCharacter->DrawObject->Rotation;

		Vector3 scale = this.Scale;
		scale.Y = pCharacter->Height * 2;
		scale.X = pCharacter->HitboxRadius * 2;
		scale.Z = pCharacter->HitboxRadius * 2;
		this.Scale = scale;
	}
}

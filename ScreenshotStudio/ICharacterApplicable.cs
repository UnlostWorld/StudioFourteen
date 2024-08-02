namespace ScreenshotStudio;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

public interface ICharacterApplicable
{
	public unsafe void Apply(Character* character);
}

public interface ICharacterRevertible
{
	public unsafe void Revert(Character* character);
}
namespace ScreenshotStudio.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

public interface ICharacterAppearance : IEntryBase
{
	public unsafe void Apply(Character* character);
}

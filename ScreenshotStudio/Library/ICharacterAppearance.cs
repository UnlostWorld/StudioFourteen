namespace ScreenshotStudio.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

public interface ICharacterAppearance
{
	string? Name { get; }
	public unsafe void Apply(Character* character);
}

public interface ICharacterEntry : ICharacterAppearance, ILibraryEntry
{
}
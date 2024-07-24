namespace ScreenshotStudio.Library;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

public interface IActorAppearance : IEntryBase
{
	public unsafe void Apply(Character* actor);
}

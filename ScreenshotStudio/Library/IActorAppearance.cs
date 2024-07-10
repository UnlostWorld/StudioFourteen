namespace ScreenshotStudio.Library;

using ScreenshotStudio.Structs;

public interface IActorAppearance : IEntryBase
{
	public unsafe void Apply(Actor* actor);
}

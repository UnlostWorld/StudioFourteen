namespace ScreenshotStudio.Library;

using ScreenshotStudio.Structs;

public interface IActorAppearance : ILibraryItem
{
	public unsafe void Apply(Actor* actor);
}

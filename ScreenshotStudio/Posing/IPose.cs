namespace ScreenshotStudio.Posing;

using FFXIVClientStructs.FFXIV.Client.Game.Character;

public interface IPose
{
	public unsafe void Apply(Character* character);
}

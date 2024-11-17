namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class DoubleColorMenu : MakeMenuViewModel
{
	public DoubleColorMenu(CharaMakeType makeType, CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex, ToggleModes toggleMode)
		: base(makeMenu, customizeIndex, toggleMode)
	{
		CustomizeIndex rightIndex = customizeIndex;

		// surely this must came from the makeMenu, but I cant find how.
		if (customizeIndex == CustomizeIndex.EyeColor)
			rightIndex = CustomizeIndex.EyeColor2;

		if (customizeIndex == CustomizeIndex.HairColor)
			rightIndex = CustomizeIndex.HairColor2;

		this.Left = new(makeType, makeMenu, customizeIndex, toggleMode, true);
		this.Right = new(makeType, makeMenu, rightIndex, toggleMode, true);
	}

	public ColorMenu Left { get; set; }
	public ColorMenu Right { get; set; }

	public override unsafe void OnFrameworkUpdate(Character* pCharacter)
	{
		base.OnFrameworkUpdate(pCharacter);
		this.Left.OnFrameworkUpdate(pCharacter);
		this.Right.OnFrameworkUpdate(pCharacter);
	}
}

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class MultiColorMenu(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex)
	: MakeMenuViewModel(makeMenu, customizeIndex)
{
}

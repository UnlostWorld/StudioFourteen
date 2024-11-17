namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class MultiColorMenu(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex, MenuViewModel.ToggleModes toggleMode)
	: MakeMenuViewModel(makeMenu, customizeIndex, toggleMode)
{
}

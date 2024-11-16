namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class PercentageMenu(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex)
	: MakeMenuViewModel(makeMenu, customizeIndex)
{
	public byte Minimum => 0;
	public byte Maximum => 100;
}
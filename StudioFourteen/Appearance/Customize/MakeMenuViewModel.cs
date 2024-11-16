namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Text.ReadOnly;
using System.Text;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public abstract class MakeMenuViewModel(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex)
	: MenuViewModel(customizeIndex)
{
	public override string? Name => makeMenu.Menu.Value.Text.GetString();
}

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;

public class GenderMenu : MenuViewModel
{
	public GenderMenu(string name)
		: base(CustomizeIndex.Gender, ToggleModes.None)
	{
		this.Name = name;
	}
}

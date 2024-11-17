namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;

public class GenderMenu : MenuViewModel
{
	private readonly string name;

	public GenderMenu(string name)
		: base(CustomizeIndex.Gender, ToggleModes.None)
	{
		this.name = name;
	}

	public override string? Name => this.name;
}

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;

public class TribeMenu : SelectorMenu
{
	public TribeMenu(Race race)
		: base(CustomizeIndex.Tribe, ToggleModes.None)
	{
		Tribe[] tribes = race.GetTribes();
		foreach (Tribe tribe in tribes)
		{
			string name = tribe.GetName() ?? tribe.ToString() ?? string.Empty;
			this.Options.Add(new Option(name, (byte)tribe.RowId));
		}
	}

	public override string? Name => "Tribe";
}

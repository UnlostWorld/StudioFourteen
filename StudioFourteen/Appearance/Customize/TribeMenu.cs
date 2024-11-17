namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using Lumina.Excel.Sheets;

public class TribeMenu : SelectorMenu
{
	private readonly string name;

	public TribeMenu(Race race, string name)
		: base(CustomizeIndex.Tribe, ToggleModes.None)
	{
		this.name = name;

		Tribe[] tribes = race.GetTribes();
		foreach (Tribe tribe in tribes)
		{
			string tribeName = tribe.GetName() ?? tribe.ToString() ?? string.Empty;
			this.Options.Add(new Option(tribeName, (byte)tribe.RowId));
		}
	}

	public override string? Name => this.name;
}

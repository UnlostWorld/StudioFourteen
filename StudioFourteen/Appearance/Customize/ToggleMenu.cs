namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;

public class ToggleMenu : SelectorMenu
{
	private readonly string? name;

	public ToggleMenu(CustomizeIndex index, string? name = null)
		: base(index, ToggleModes.IsToggle)
	{
		this.name = name;

		this.Options.Add(new("Disabled", 0));
		this.Options.Add(new("Enabled", 1));
	}

	public override string? Name => this.name;
}

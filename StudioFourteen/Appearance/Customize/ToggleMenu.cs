namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;

public class ToggleMenu : SelectorMenu
{
	private readonly string? name;

	public ToggleMenu(CustomizeIndex index, string? name = null)
		: base(index, ToggleModes.IsToggle)
	{
		this.name = name;
	}

	public bool IsChecked
	{
		get => this.Value == 1;
		set => this.Value = (byte)(value ? 1 : 0);
	}

	public override string? Name => this.name;
}

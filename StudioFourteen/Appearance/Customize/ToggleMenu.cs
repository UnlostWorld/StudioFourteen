namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;

public class ToggleMenu : MenuViewModel
{
	public ToggleMenu(CustomizeIndex index, MenuViewModel? innerMenu)
		: base(index, ToggleModes.IsToggle)
	{
		if (innerMenu != null)
		{
			this.InnerMenu = innerMenu;
			this.Name = innerMenu.Name;
			innerMenu.Name = null;
		}
	}

	public bool IsChecked
	{
		get => this.Value == 1;
		set => this.Value = (byte)(value ? 1 : 0);
	}

	public MenuViewModel? InnerMenu { get; private set; }

	public override unsafe void OnFrameworkUpdate(Character* pCharacter)
	{
		base.OnFrameworkUpdate(pCharacter);
		this.InnerMenu?.OnFrameworkUpdate(pCharacter);
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);
		this.RaisePropertyChanged(nameof(this.IsChecked));
	}
}

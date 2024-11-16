namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;

public class RaceMenu()
	: MenuViewModel(CustomizeIndex.Race)
{
	private RaceLibraryEntry? race;

	public override string? Name => "Race";

	public RaceLibraryEntry? Race
	{
		get => this.race;
		set
		{
			this.race = value;

			if (value == null)
			{
				this.Value = (byte)RaceRows.Hyur;
			}
			else
			{
				this.Value = (byte)value.RowId;
			}
		}
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		this.race = this.Services.GameData.GetLibraryEntry<RaceLibraryEntry>(newValue);
		this.RaisePropertyChanged(nameof(this.Race));
	}
}

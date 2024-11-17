namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using WpfUtils.Extensions;

public abstract class SelectorMenu : MenuViewModel
{
	private Option? selected;

	public SelectorMenu(CustomizeIndex customizeIndex, ToggleModes toggleMode)
		: base(customizeIndex, toggleMode)
	{
		this.OnValueChanged(0, this.Value);
	}

	public Option? Selected
	{
		get => this.selected;
		set
		{
			this.selected = value;
			this.RaisePropertyChanged(nameof(this.Selected));

			if (this.selected == null)
			{
				this.Value = 0;
			}
			else
			{
				this.Value = this.selected.Value;
			}
		}
	}

	public FastObservableCollection<Option> Options { get; init; } = new();

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		foreach (Option op in this.Options)
		{
			if (op.Value == newValue)
			{
				this.selected = op;
				this.RaisePropertyChanged(nameof(this.Selected));
				break;
			}
		}
	}

	public record Option(string Label, byte Value);
}

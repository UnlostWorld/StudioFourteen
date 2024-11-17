namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.GameData;
using System.Collections.Generic;
using System.Windows.Media;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class ColorMenu : MakeMenuViewModel
{
	private Option? selected;

	public ColorMenu(CharaMakeType makeType, CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex, ToggleModes toggleMode, bool hideName = false)
		: base(makeMenu, customizeIndex, toggleMode)
	{
		this.Options = new();

		HumanCmp.Entry[]? entries = HumanCmp.Get(makeType, makeMenu);
		if (entries == null)
			return;

		for (byte i = 0; i < entries.Length; i++)
		{
			if (entries[i].Skip)
				continue;

			this.Options.Add(new(entries[i].Color, i, entries[i].Hex));
		}

		if (hideName)
		{
			this.Name = null;
		}
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

	public List<Option> Options { get; }

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

	public record Option(Color Color, byte Value, string Hex);
}

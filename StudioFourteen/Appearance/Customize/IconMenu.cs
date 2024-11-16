namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.GameData;
using System.Collections.Generic;
using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class IconMenu : MakeMenuViewModel
{
	private Option? selected;

	public IconMenu(CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex customizeIndex)
		: base(makeMenu, customizeIndex)
	{
		this.Options = new();

		for (byte i = 1; i <= makeMenu.SubMenuNum; i++)
		{
			this.Options.Add(new(new ImageReference(makeMenu.SubMenuParam[i - 1]), i));
		}

		this.Minimum = 1;
		this.Maximum = this.Options[makeMenu.SubMenuNum - 1].Value;
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
	public byte Minimum { get; private set; }
	public byte Maximum { get; private set; }

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

	public record Option(ImageReference Image, byte Value);
}

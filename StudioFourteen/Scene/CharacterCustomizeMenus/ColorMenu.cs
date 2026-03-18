// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Scene.CharacterCustomizeMenus;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.Services.Library.GameData;
using System.Collections.Generic;
using StudioFourteen.Services.Rendering;

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;

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
			this.OnPropertyChanged(nameof(this.Selected));

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
				this.OnPropertyChanged(nameof(this.Selected));
				break;
			}
		}
	}

	public record Option(Color Color, byte Value, string Hex);
}

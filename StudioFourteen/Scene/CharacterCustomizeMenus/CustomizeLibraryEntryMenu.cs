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
using Lumina.Excel;
using Lumina.Text.ReadOnly;
using StudioFourteen.Services.Library.GameData;
using StudioFourteen.Services.Library.GameData.Library;
using System.Collections.Generic;

using CharaMakeType = StudioFourteen.Services.Library.GameData.Sheets.CharaMakeType;

public class CustomizeLibraryEntryMenu : CharacterCustomizeMenu
{
	public CharaMakeType MakeType;
	public CharaMakeType.CharaMakeMenu MakeMenu;

	private CharaMakeCustomizeLibraryEntry? entry;

	public CustomizeLibraryEntryMenu(CharaMakeType makeType, CharaMakeType.CharaMakeMenu makeMenu, CustomizeIndex index, bool canToggle)
		: base(index, canToggle ? ToggleModes.IsValue : ToggleModes.None)
	{
		this.MakeType = makeType;
		this.MakeMenu = makeMenu;

		this.Name = this.MakeMenu.Menu.Value.Text.GetString();

		if (canToggle)
		{
			this.ToggleMenu = new ToggleMenu(index, null);
		}
	}

	public CharaMakeCustomizeLibraryEntry? Entry
	{
		get => this.entry;
		set
		{
			if (value == null)
				return;

			if (this.MakeType.Race.IsRow(value.Race)
				&& this.MakeType.Tribe.IsRow(value.Tribe)
				&& this.MakeType.Gender == (sbyte)value.Gender)
			{
				this.entry = value;
				this.Value = (byte)value.MakeCustomize.Value.FeatureID;
				this.OnPropertyChanged(nameof(this.Entry));
			}
		}
	}

	public ToggleMenu? ToggleMenu { get; init; }

	public override void OnGameTick(CharacterCustomize character)
	{
		base.OnGameTick(character);
		this.ToggleMenu?.OnGameTick(character);
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		List<CharaMakeCustomizeLibrarySource> sources = Studio.Library.GetSources<CharaMakeCustomizeLibrarySource>();
		foreach (CharaMakeCustomizeLibrarySource source in sources)
		{
			this.entry = source.Find(
				this.MakeType.Race.Value,
				this.MakeType.Tribe.Value,
				(Genders)this.MakeType.Gender,
				this.CustomizeIndex,
				newValue);

			if (this.entry != null)
			{
				break;
			}
		}

		this.OnPropertyChanged(nameof(this.Entry));
	}
}

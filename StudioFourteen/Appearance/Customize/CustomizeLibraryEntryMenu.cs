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

namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Extensions;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using StudioFourteen.Tags;
using System.Collections.Generic;

using CharaMakeType = StudioFourteen.GameData.Sheets.CharaMakeType;

public class CustomizeLibraryEntryMenu : MenuViewModel
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

		this.SearchTags.Add(makeType.Race.Value.ToTags());
		this.SearchTags.Add(makeType.Tribe.Value.ToTags());
		this.SearchTags.Add(((Genders)makeType.Gender).ToTags());
		this.SearchTags.Add(index.ToTag());

		if (canToggle)
		{
			this.ToggleMenu = new ToggleMenu(index, null);
		}
	}

	public TagCollection SearchTags { get; init; } = new();

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
				this.RaisePropertyChanged(nameof(this.Entry));
			}
		}
	}

	public ToggleMenu? ToggleMenu { get; init; }

	public override unsafe void OnGameTick(Character* pCharacter)
	{
		base.OnGameTick(pCharacter);
		this.ToggleMenu?.OnGameTick(pCharacter);
	}

	protected override void OnValueChanged(byte oldValue, byte newValue)
	{
		base.OnValueChanged(oldValue, newValue);

		List<CharaMakeCustomizeLibrarySource> sources = this.Services.Library.GetSources<CharaMakeCustomizeLibrarySource>();
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

		this.RaisePropertyChanged(nameof(this.Entry));
	}
}

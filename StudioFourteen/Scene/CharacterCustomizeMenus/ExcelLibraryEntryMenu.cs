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
using StudioFourteen.Services.Library.GameData.Library;
using System;

public abstract class ExcelLibraryEntryMenu(CustomizeIndex index)
	: CharacterCustomizeMenu(index, ToggleModes.None)
{
}

public class ExcelLibraryEntryMenu<T> : ExcelLibraryEntryMenu
	where T : ExcelLibraryEntry
{
	private T? entry;

	public ExcelLibraryEntryMenu(CustomizeIndex index, string name)
		: base(index)
	{
		this.Name = name;
	}

	public Type EntryType => typeof(T);

	public T? Entry
	{
		get => this.entry;
		set
		{
			this.entry = value;

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

		this.entry = Studio.Library.GameData.GetLibraryEntry<T>(newValue);
		this.OnPropertyChanged(nameof(this.Entry));
	}
}

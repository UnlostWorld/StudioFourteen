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

using CommunityToolkit.Mvvm.ComponentModel;
using Dalamud.Game.ClientState.Objects.Enums;

public abstract class CharacterCustomizeMenu : ObservableObject
{
	protected readonly CustomizeIndex CustomizeIndex;
	protected readonly ToggleModes ToggleMode;

	private byte lastReadValue;
	private byte? nextWriteValue;
	private bool isFirstRead = true;

	public CharacterCustomizeMenu(CustomizeIndex customizeIndex, ToggleModes toggleMode)
	{
		this.CustomizeIndex = customizeIndex;
		this.ToggleMode = toggleMode;
	}

	public enum ToggleModes
	{
		None,
		IsValue,
		IsToggle,
	}

	public string? Name { get; set; }

	public byte RealValue
	{
		get => this.nextWriteValue ?? this.lastReadValue;
		set
		{
			if (value == this.RealValue)
				return;

			this.nextWriteValue = value;
			this.OnPropertyChanged(nameof(this.RealValue));
			this.OnPropertyChanged(nameof(this.Value));
			this.OnValueChanged(this.lastReadValue, this.Value);
		}
	}

	public byte Value
	{
		get
		{
			byte value = this.RealValue;

			if (this.ToggleMode == ToggleModes.IsToggle)
			{
				if (value >= 128)
				{
					value = 1;
				}
				else
				{
					value = 0;
				}
			}
			else if (this.ToggleMode == ToggleModes.IsValue)
			{
				if (value > 128)
				{
					value -= 128;
				}
			}

			return value;
		}
		set
		{
			if (this.ToggleMode == ToggleModes.IsToggle)
			{
				if (this.RealValue >= 128 && value == 0)
				{
					value = (byte)(this.RealValue - 128);
				}
				else if (this.RealValue < 128 && value == 1)
				{
					value = (byte)(this.RealValue + 128);
				}
			}
			else if (this.ToggleMode == ToggleModes.IsValue)
			{
				if (this.RealValue >= 128)
				{
					value = (byte)(value + 128);
				}
			}

			this.RealValue = value;
		}
	}

	public unsafe virtual void OnGameTick(CharacterCustomize character)
	{
		byte oldValue = this.Value;
		this.lastReadValue = character.GetCustomizeValue(this.CustomizeIndex);

		if (this.nextWriteValue != null)
		{
			if (this.nextWriteValue.Value != this.lastReadValue)
				character.SetCustomizeValue(this.CustomizeIndex, this.nextWriteValue.Value, UpdateSource.Interface);

			this.lastReadValue = this.nextWriteValue.Value;
			this.nextWriteValue = null;
		}

		if (this.Value != oldValue || this.isFirstRead)
		{
			this.OnPropertyChanged(nameof(this.Value));
			this.OnValueChanged(oldValue, this.Value);
		}

		this.isFirstRead = false;
	}

	protected virtual void OnValueChanged(byte oldValue, byte newValue)
	{
	}
}
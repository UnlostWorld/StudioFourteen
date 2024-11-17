namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Mvm;

public abstract class MenuViewModel
	: ViewModel
{
	protected readonly CustomizeIndex CustomizeIndex;
	protected readonly ToggleModes ToggleMode;

	private byte lastReadValue;
	private byte? nextWriteValue;
	private bool isFirstRead = true;

	public MenuViewModel(CustomizeIndex customizeIndex, ToggleModes toggleMode)
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

	public abstract string? Name { get; }

	public byte RealValue
	{
		get => this.nextWriteValue ?? this.lastReadValue;
		set
		{
			if (value == this.RealValue)
				return;

			this.nextWriteValue = value;
			this.RaisePropertyChanged(nameof(this.Value));
			this.OnValueChanged(this.lastReadValue, value);
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

	public unsafe virtual void OnFrameworkUpdate(Character* pCharacter)
	{
		byte oldValue = this.Value;
		this.lastReadValue = pCharacter->GetCustomizeValue(this.CustomizeIndex);

		if (this.nextWriteValue != null)
		{
			if (this.nextWriteValue.Value != this.lastReadValue)
				pCharacter->SetCustomizeValue(this.CustomizeIndex, this.nextWriteValue.Value, CharacterExtensions.UpdateSource.Interface, true);

			this.lastReadValue = this.nextWriteValue.Value;
			this.nextWriteValue = null;
		}

		if (this.Value != oldValue || this.isFirstRead)
		{
			this.RaisePropertyChanged(nameof(this.Value));
			this.OnValueChanged(oldValue, this.Value);
		}

		this.isFirstRead = false;
	}

	protected virtual void OnValueChanged(byte oldValue, byte newValue)
	{
	}
}
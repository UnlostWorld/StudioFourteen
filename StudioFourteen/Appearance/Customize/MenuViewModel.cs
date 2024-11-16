namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Mvm;

public abstract class MenuViewModel(CustomizeIndex customizeIndex)
	: ViewModel
{
	private byte lastReadValue;
	private byte? nextWriteValue;
	private bool isFirstRead = true;

	public abstract string? Name { get; }

	public byte Value
	{
		get => this.nextWriteValue ?? this.lastReadValue;
		set
		{
			if (value == this.Value)
				return;

			this.nextWriteValue = value;
			this.RaisePropertyChanged(nameof(this.Value));
			this.OnValueChanged(this.lastReadValue, value);
		}
	}

	public unsafe virtual void OnFrameworkUpdate(Character* pCharacter)
	{
		byte oldValue = this.Value;
		this.lastReadValue = pCharacter->GetCustomizeValue(customizeIndex);

		if (this.nextWriteValue != null)
		{
			if (this.nextWriteValue.Value != this.lastReadValue)
				pCharacter->SetCustomizeValue(customizeIndex, this.nextWriteValue.Value, CharacterExtensions.UpdateSource.Interface, true);

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
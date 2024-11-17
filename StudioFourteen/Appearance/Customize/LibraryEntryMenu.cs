namespace StudioFourteen.Appearance.Customize;

using Dalamud.Game.ClientState.Objects.Enums;
using StudioFourteen.GameData;
using StudioFourteen.GameData.Library;
using System;

public abstract class LibraryEntryMenu(CustomizeIndex index)
	: MenuViewModel(index, ToggleModes.None)
{
}

public class LibraryEntryMenu<T>(CustomizeIndex index, string name) : LibraryEntryMenu(index)
	where T : ExcelLibraryEntry
{
	private T? entry;

	public override string? Name => name;

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

		this.entry = this.Services.GameData.GetLibraryEntry<T>(newValue);
		this.RaisePropertyChanged(nameof(this.Entry));
	}
}

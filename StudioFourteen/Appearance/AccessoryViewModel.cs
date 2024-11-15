namespace StudioFourteen.Appearance;

using StudioFourteen.GameData.Library;

public enum AccessorySlots
{
	Glasses,
}

public class AccessoryViewModel : ExcelRowItemViewModel<GlassesLibraryEntry>
{
	public AccessoryViewModel(AccessorySlots slot)
	{
		this.Slot = slot;
	}

	public AccessorySlots Slot { get; init; }

	protected unsafe override ushort LiveValue
	{
		get => this.Target->DrawData.GlassesIds[(int)this.Slot];
		set => this.Target->DrawData.SetGlasses((int)this.Slot, value);
	}

	protected override string GetSearchTitle() => Resources.Find("LOC_Glasses", "Glasses");
}

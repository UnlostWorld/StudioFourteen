namespace StudioFourteen.Appearance;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance.Customize;
using StudioFourteen.Files;
using StudioFourteen.GameData.Library;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.Windows;
using System.Windows.Input;
using WpfUtils.Extensions;

public partial class CharacterPanel : CharacterPanelBase
{
	public CharacterPanel()
	{
		this.Customize = new(this);
	}

	public CustomizeViewModel Customize { get; init; }

	[AutoNotify] public bool UseTwoColumns => this.ActualWidth > 650;

	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex);
	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(StudioFourteen.Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.CharacterName);

	[AutoNotify]
	public int SelectedTab
	{
		get => this.GetPersistence<int>();
		set => this.SetPersistence(value);
	}

	[AutoNotify] public WeaponViewModel MainHand { get; init; } = new(DrawDataContainer.WeaponSlot.MainHand);
	[AutoNotify] public WeaponViewModel OffHand { get; init; } = new(DrawDataContainer.WeaponSlot.OffHand);

	[AutoNotify] public ItemEquipViewModel Head { get; init; } = new(DrawDataContainer.EquipmentSlot.Head);
	[AutoNotify] public ItemEquipViewModel Chest { get; init; } = new(DrawDataContainer.EquipmentSlot.Body);
	[AutoNotify] public ItemEquipViewModel Hands { get; init; } = new(DrawDataContainer.EquipmentSlot.Hands);
	[AutoNotify] public ItemEquipViewModel Legs { get; init; } = new(DrawDataContainer.EquipmentSlot.Legs);
	[AutoNotify] public ItemEquipViewModel Feet { get; init; } = new(DrawDataContainer.EquipmentSlot.Feet);
	[AutoNotify] public ItemEquipViewModel Earring { get; init; } = new(DrawDataContainer.EquipmentSlot.Ears);
	[AutoNotify] public ItemEquipViewModel Necklace { get; init; } = new(DrawDataContainer.EquipmentSlot.Neck);
	[AutoNotify] public ItemEquipViewModel Bracelet { get; init; } = new(DrawDataContainer.EquipmentSlot.Wrists);
	[AutoNotify] public ItemEquipViewModel RingRight { get; init; } = new(DrawDataContainer.EquipmentSlot.RFinger);
	[AutoNotify] public ItemEquipViewModel RingLeft { get; init; } = new(DrawDataContainer.EquipmentSlot.LFinger);

	public AccessoryViewModel Glasses { get; init; } = new(AccessorySlots.Glasses);
	public OrnamentViewModel Ornament { get; init; } = new();

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		////MiniLibraryPopOut.Close();

		base.OnPreviewMouseDown(e);
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearance.Restore(this.TargetObjectIndex).Run();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryWindow.Open();
	}

	private void OnExportClicked(object sender, RoutedEventArgs e)
	{
		AppearanceFile file = new();
		file.Read(this.TargetObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.CharacterName}'s Appearance");
	}
}

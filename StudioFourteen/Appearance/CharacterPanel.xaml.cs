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

namespace StudioFourteen.Appearance;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Appearance.Customize;
using StudioFourteen.Appearance.Equipment;
using StudioFourteen.Files;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using System.Windows;
using WpfUtils.Extensions;

public partial class CharacterPanel : CharacterPanelBase
{
	public CharacterPanel()
	{
		this.Customize = new(this);
		this.Equipment = new();
	}

	public CustomizeViewModel Customize { get; init; }
	public EquipmentViewModel Equipment { get; init; }

	[AutoNotify] public bool UseTwoColumns => this.ActualWidth > 650;

	[AutoNotify] public unsafe bool CanRevert => this.Services.CharacterAppearance.CanRestore(this.TargetObjectIndex);
	[AutoNotify] public string ExportAppearanceToolTipText => string.Format(StudioFourteen.Resources.Find("LOC_Save_ExportAppearanceToolTip", string.Empty), this.CharacterName);

	[AutoNotify]
	public int SelectedTab
	{
		get
		{
			int tabIndex = this.GetPersistence<int>();
			if (tabIndex == 3 && this.UseTwoColumns)
			{
				tabIndex = 0;
			}

			return tabIndex;
		}
		set => this.SetPersistence(value);
	}

	protected unsafe override void OnGameTick()
	{
		base.OnGameTick();

		Character* pTarget = this.Services.Target.GetTarget();
		if (pTarget == null)
			return;

		this.Customize.OnGameTick(pTarget);
		this.Equipment.OnGameTick(pTarget);
	}

	protected override void OnTargetChanged(int objectTableIndex)
	{
		base.OnTargetChanged(objectTableIndex);

		this.Customize.OnTargetChanged();
		this.Equipment.OnTargetChanged();
	}

	private unsafe void OnRevertClicked(object sender, RoutedEventArgs e)
	{
		this.Services.CharacterAppearance.Restore(this.TargetObjectIndex).Run();
	}

	private void OnImportClicked(object sender, RoutedEventArgs e)
	{
		LibraryPanel.Open(this.GetContext());
	}

	private async void OnExportClicked(object sender, RoutedEventArgs e)
	{
		AppearanceFile file = new();
		await file.Read(this.TargetObjectIndex);
		this.Services.Files.SaveFile(file, $"{this.CharacterName}'s Appearance");
	}
}

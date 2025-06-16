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

using StudioFourteen.Panels;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Scene.GameObjects.Characters;

public partial class CharacterPanel : Panel
{
	//// public EquipmentViewModel Equipment { get; init; }

	[Notify] private Character? character;

	protected override void OnOpened()
	{
		this.Services.Selection.GetScope<Character>().Attach(this.OnSelectionChanged);
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Selection.GetScope<Character>().Detach(this.OnSelectionChanged);
		base.OnClosed();
	}

	private void OnSelectionChanged(Character? newSelection, object? selectionSource)
	{
		this.Character = newSelection;
	}

	/*protected unsafe override void OnGameTick()
	{
		base.OnGameTick();

		if (this.GameObject == null)
			return;

		Character* pTarget = (Character*)this.GameObject.GetXivGameObject();
		if (pTarget == null)
			return;

		this.Equipment.OnGameTick(pTarget);
	}

	protected override void OnTargetChanged(int objectTableIndex)
	{
		base.OnTargetChanged(objectTableIndex);

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
	}*/
}

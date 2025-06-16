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

namespace StudioFourteen.Panels;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Scene.GameObjects.Characters;

public abstract partial class CharacterPanelBase : Panel
{
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

	protected virtual void OnTargetChanged(int objectTableIndex)
	{
	}

	private void OnSelectionChanged(Character? newSelection, object? selectionSource)
	{
		this.Character = newSelection;
	}
}
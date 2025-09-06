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
using StudioFourteen.Selection;

public abstract partial class CharacterPanelBase : Panel
{
	private readonly SelectionListener<Character> characterSelectionListener;

	[Notify] private Character? character;

	public CharacterPanelBase()
	{
		this.characterSelectionListener = new(this.OnSelectionChanged);
	}

	protected override void OnOpened()
	{
		this.characterSelectionListener.Enable();
		this.OnSelectionChanged(null, this.characterSelectionListener.Current, this);
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.characterSelectionListener.Disable();
		base.OnClosed();
	}

	protected virtual void OnTargetChanged(Character? newTarget)
	{
	}

	private void OnSelectionChanged(Character? oldSelection, Character? newSelection, object? selectionSource)
	{
		this.Character = newSelection;
		this.OnTargetChanged(newSelection);
	}
}
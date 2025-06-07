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

namespace StudioFourteen.Rendering.Scene.Gizmos;

using StudioFourteen.Selection;

public abstract class SelectionGizmoBase : GizmoBase
{
	protected SelectionBase? selection;

	public void Enable(SelectionBase selection)
	{
		this.selection = selection;
		this.Enable();
	}

	public abstract bool SupportsSelection(SelectionBase selection);

	public virtual void OnGameTick()
	{
	}
}

public abstract class SelectionGizmoBase<TSelectionType> : SelectionGizmoBase
	where TSelectionType : SelectionBase
{
	public TSelectionType? Selection
	{
		get
		{
			if (this.selection is TSelectionType tSelection)
				return tSelection;

			return null;
		}
	}

	public override bool SupportsSelection(SelectionBase selection)
	{
		return typeof(TSelectionType).IsAssignableFrom(selection.GetType());
	}
}
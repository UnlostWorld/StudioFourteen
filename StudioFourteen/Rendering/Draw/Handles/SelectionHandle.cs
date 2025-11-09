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

namespace StudioFourteen.Rendering.Draw.Handles;

using StudioFourteen.Scene;

public class SelectionHandle : Handle
{
	private readonly SceneObjectBase selection;

	public SelectionHandle(SceneObjectBase selection)
	{
		this.selection = selection;

		SelectionService.HoverChanged += this.OnSelectionHoverChanged;
		SelectionService.SelectionChanged += this.OnSelectionChanged;
	}

	public bool IsSelected { get; private set; }

	protected SceneObjectBase Selection => this.selection;

	public sealed override void SetIsHandleHovered(bool hover)
	{
		if (hover)
		{
			SelectionService.HoverSelection(this.selection, this);
			SelectionService.HoverSource = this;
		}
		else if (SelectionService.Hover?.Id == this.selection.Id)
		{
			SelectionService.ClearHover();
		}

		base.SetIsHandleHovered(hover);
	}

	protected override void OnIsPressedChanged(bool isPressed)
	{
		base.OnIsPressedChanged(isPressed);

		if (!isPressed)
		{
			if (SelectionService.Current == this.selection)
			{
				SelectionService.ExpandedSelection = true;
			}
			else
			{
				SelectionService.ExpandedSelection = false;
				SelectionService.Select(this.selection, this);
			}
		}
	}

	protected override void OnIsHoveredChanged(bool isHovered)
	{
		base.OnIsHoveredChanged(isHovered);
	}

	private void OnSelectionHoverChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		if (oldSelection?.Id == this.selection.Id)
		{
			this.OnIsHoveredChanged(false);
		}
		else if (newSelection?.Id == this.selection.Id)
		{
			this.OnIsHoveredChanged(true);
		}
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.IsSelected = newSelection?.Id == this.selection.Id;
	}
}
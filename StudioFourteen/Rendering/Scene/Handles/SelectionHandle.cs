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

namespace StudioFourteen.Rendering.Scene.Handles;

using StudioFourteen.Selection;

public class SelectionHandle : Handle
{
	private readonly SelectionBase selection;

	public SelectionHandle(SelectionBase selection)
	{
		this.selection = selection;

		this.Services.Selection.HoverChanged += this.OnSelectionHoverChanged;
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
	}

	public bool IsSelected { get; private set; }

	public sealed override void SetIsHandleHovered(bool hover)
	{
		if (hover)
		{
			this.Services.Selection.Hover = this.selection;
			this.Services.Selection.HoverSource = this;
		}
		else if (this.Services.Selection.Hover?.Id == this.selection.Id)
		{
			this.Services.Selection.Hover = null;
		}

		base.SetIsHandleHovered(hover);
	}

	protected override void OnIsPressedChanged(bool isPressed)
	{
		base.OnIsPressedChanged(isPressed);

		if (!isPressed)
		{
			if (this.Services.Selection.Current == this.selection)
			{
				this.Services.Selection.ExpandedSelection = true;
			}
			else
			{
				this.Services.Selection.ExpandedSelection = false;
				this.Services.Selection.Current = this.selection;
			}
		}
	}

	protected override void OnIsHoveredChanged(bool isHovered)
	{
		base.OnIsHoveredChanged(isHovered);
	}

	private void OnSelectionHoverChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
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

	private void OnSelectionChanged(SelectionBase? oldSelection, SelectionBase? newSelection)
	{
		this.IsSelected = newSelection?.Id == this.selection.Id;
	}
}
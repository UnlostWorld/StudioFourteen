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

namespace StudioFourteen.Selection;

using StudioFourteen.Overlays;
using System.Windows.Controls;

public abstract class SelectionOverlayLayerBase : OverlayLayerBase
{
	public SelectionOverlayLayerBase(string name)
		: base("Selection", name)
	{
	}

	protected SelectionBase? Selection => this.Services.Selection.Selection;
	protected int TargetIndex => this.Services.Target.TargetObjectIndex;

	public override void Enable(Canvas canvas)
	{
		base.Enable(canvas);

		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Target.TargetChanged += this.OnTargetChanged;
	}

	public override void Disable(Canvas canvas)
	{
		base.Disable(canvas);

		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
	}

	protected virtual void OnSelectionChanged(SelectionBase? newSelection)
	{
	}

	protected virtual void OnTargetChanged(int objectTableIndex)
	{
	}
}
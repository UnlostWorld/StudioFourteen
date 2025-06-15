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
using StudioFourteen.Mvm;
using StudioFourteen.Scene;
using StudioFourteen.Scene.GameObjects;

public abstract partial class CharacterPanelBase : Panel
{
	[Notify] private GameObject? gameObject;

	[AlwaysNotify] public bool IsTargetLoading => !this.gameObject?.IsReady == true;
	[AlwaysNotify] public string? CharacterName => this.gameObject?.Name;
	[AlwaysNotify] public bool HasValidTarget => this.gameObject != null;
	[AlwaysNotify] public int TargetObjectIndex => this.gameObject?.ObjectIndex ?? 0;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	protected override void OnOpened()
	{
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		base.OnClosed();
	}

	protected virtual void OnTargetChanged(int objectTableIndex)
	{
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? selectionSource)
	{
		if (newSelection is GameObject gameObject)
		{
			this.GameObject = gameObject;
			this.OnTargetChanged(gameObject.ObjectIndex);
		}
	}
}
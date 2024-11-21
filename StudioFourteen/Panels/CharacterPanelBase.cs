// .                      @@             _____ _______ _    _ _____ _____ ____
//            @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//           @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//           @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//          @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//      @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//       @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//        @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//        @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//      @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//       @@@@             @@@
//         @@@@@      @@@@@               This software is licensed under the
//          @@@@@@@@@@@@@@                 GNU AFFERO GENERAL PUBLIC LICENSE
//              @@@@  @                       Version 3, 19 November 2007

namespace StudioFourteen.Panels;

using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Mvm;

public abstract class CharacterPanelBase : Panel
{
	[AlwaysNotify] public bool IsTargetLoading => this.Services.Target.IsTargetLoading;
	[AlwaysNotify] public string? CharacterName => this.Services.Target.CharacterName;
	[AlwaysNotify] public bool HasValidTarget => this.Services.Target.HasValidTarget;
	[AlwaysNotify] public int TargetObjectIndex => this.Services.Target.TargetObjectIndex;

	public override bool ShouldTickAutoProperties()
	{
		if (!this.HasValidTarget)
			return false;

		return base.ShouldTickAutoProperties();
	}

	protected override void OnOpened()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		base.OnOpened();
	}

	protected override void OnClosed()
	{
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		base.OnClosed();
	}

	protected virtual void OnTargetChanged()
	{
	}
}
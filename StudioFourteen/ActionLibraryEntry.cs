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

namespace StudioFourteen.GameData.Library;

using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Animation;
using StudioFourteen.GameData.Extensions;
using StudioFourteen.Icons;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;

public class ActionLibraryEntry : ExcelLibraryEntry, ITimelineAnimation
{
	public readonly Action Action;

	public ActionLibraryEntry(SourceBase source, Action action)
		: base(source, action.RowId)
	{
		this.Action = action;

		if (action.IsPlayerAction)
		{
			this.Tags.Add("Player Action");
			this.Tags.Add("Player Animation");
		}

		if (action.IsPvP)
			this.Tags.Add("PvP");

		if (action.CanUseWhileMounted)
			this.Tags.Add("Mounted");

		if (action.ClassJob.IsValid)
		{
			this.Tags.Add(action.ClassJob.Value.ToTags());
			this.Tags.Add("Player Animation");
		}

		if (action.IsRoleAction)
			this.Tags.Add("Role");
	}

	public override bool IsValid
	{
		get
		{
			if (!base.IsValid)
				return false;

			if (this.Action.AnimationStart.RowId == 0
				&& this.Action.AnimationEnd.RowId == 0
				&& this.Action.ActionTimelineHit.RowId == 0)
				return false;

			return true;
		}
	}

	public override string? Name => this.Action.Name.GetString();
	public override object? Icon
	{
		get
		{
			// Icon 405 (cure) is the default, so set it to 0 instead
			// of flooding the library with cure icons.
			if (!this.Action.IsPlayerAction && this.Action.Icon == 405)
				return new ImageReference(0);

			return new ImageReference(this.Action.Icon);
		}
	}

	public ushort IntroTimelineId => (ushort)this.Action.AnimationEnd.RowId;
	public ushort LoopTimelineId => (ushort)this.Action.AnimationEnd.RowId;
}

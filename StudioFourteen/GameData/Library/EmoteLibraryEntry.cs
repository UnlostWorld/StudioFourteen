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
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using StudioFourteen.Animation;
using StudioFourteen.Library.LibraryMenu;
using StudioFourteen.Library.Sources;
using StudioFourteen.Services;

public class EmoteLibraryEntry : ExcelLibraryEntry, ITimelineAnimation
{
	public readonly Emote Emote;

	public EmoteLibraryEntry(SourceBase source, Emote emote)
		: base(source, emote.RowId)
	{
		this.Emote = emote;
		this.Tags.Add("Player Animation");
	}

	public enum EmoteTimelineSlot : uint
	{
		Standard,
		Intro,
		Ground,
		Chair,
		Blend,
		Expression,
		ShortTarget,
	}

	public override string? Name => this.Emote.Name.GetString();
	public override object? Icon => new ImageReference(this.Emote.Icon);

	public override bool IsValid
	{
		get
		{
			if (!base.IsValid)
				return false;

			foreach(var rowRef in this.Emote.ActionTimeline)
			{
				if (rowRef.RowId != 0)
				{
					return true;
				}
			}

			return false;
		}
	}

	// TODO: Support for sit groundsit etc slots
	public ushort LoopTimelineId => (ushort)this.Emote.ActionTimeline[(int)EmoteTimelineSlot.Standard].RowId;
	public ushort IntroTimelineId => (ushort)this.Emote.ActionTimeline[(int)EmoteTimelineSlot.Intro].RowId;

	/*
	private static EmoteTimelineSlot GetTimelineSlotForPose(EmoteController.PoseType poseKind)
	{
		switch (poseKind)
		{
			case EmoteController.PoseType.Sit: return EmoteTimelineSlot.Chair;
			case EmoteController.PoseType.GroundSit: return EmoteTimelineSlot.Ground;
			case EmoteController.PoseType.Doze:
			case EmoteController.PoseType.Umbrella:
			case EmoteController.PoseType.Accessory:
			case EmoteController.PoseType.Idle:
			case EmoteController.PoseType.WeaponDrawn: return EmoteTimelineSlot.Standard;
		}

		throw new NotSupportedException();
	}
	*/
}
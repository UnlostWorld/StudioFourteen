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

namespace StudioFourteen.Animation;

using System;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using Action = Lumina.Excel.Sheets.Action;
using ActionTimeline = StudioFourteen.GameData.Sheets.ActionTimeline;

public class AnimationService : ServiceBase
{
	public enum TimelineSlots : uint
	{
		Base = 0,
		UpperBody = 1,
		Facial = 2,
		Add = 3,
		Unk1 = 4,
		Unk2 = 5,
		Unk3 = 6,
		Lips = 7,
		Parts1 = 8,
		Parts2 = 9,
		Parts3 = 10,
		Parts4 = 11,
		Overlay = 12,
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

	public override void Attach()
	{
		base.Attach();
	}

	public async Task PlayEmoteAsync(ushort emoteId, int objectIndex)
	{
		await TickService.GameTick();
		this.PlayEmote(emoteId, objectIndex);
	}

	public unsafe void PlayEmote(ushort emoteId, int objectIndex)
	{
		TickService.VerifyGameTickThread();

		Emote? emote = this.Services.GameData.GetRow<Emote>(emoteId);
		if (emote == null)
			return;

		Character* pCharacter = this.Services.GameObjects.Get<Character>(objectIndex);
		EmoteController.PoseType poseKind = (EmoteController.PoseType)pCharacter->EmoteController.GetPoseKind();
		EmoteTimelineSlot timeLineSlot = this.GetTimelineSlotForPose(poseKind);

		uint introTimelineId = emote.Value.ActionTimeline[(int)EmoteTimelineSlot.Intro].RowId;
		uint timelineId = emote.Value.ActionTimeline[(int)timeLineSlot].RowId;
		if (timelineId == 0)
			timelineId = emote.Value.ActionTimeline[(int)EmoteTimelineSlot.Standard].RowId;

		pCharacter->Timeline.TimelineSequencer.PlayTimeline((ushort)timelineId);
	}

	private EmoteTimelineSlot GetTimelineSlotForPose(EmoteController.PoseType poseKind)
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
}
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
using System.Collections.Generic;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Animation;
using FFXIVClientStructs.Havok.Animation.Playback;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using TerraFX.Interop.Windows;
using Action = Lumina.Excel.Sheets.Action;
using ActionTimeline = StudioFourteen.GameData.Sheets.ActionTimeline;

public partial class AnimationService : ServiceBase
{
	private readonly Dictionary<int, AnimationController> controllers = new();

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

	public AnimationController GetController(int objectIndex)
	{
		if (!this.controllers.ContainsKey(objectIndex))
			this.controllers[objectIndex] = new(objectIndex);

		return this.controllers[objectIndex];
	}

	public unsafe override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		Hooks.CalculateAndApplyOverallSpeedHook.Enable(this.CalculateAndApplyOverallSpeed);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		Hooks.CalculateAndApplyOverallSpeedHook.Disable();
		base.Detach();
	}

	private void OnGameTick()
	{
		foreach((int objectIndex, AnimationController controller) in this.controllers)
		{
			controller.OnGameTick();
		}
	}

	private unsafe bool CalculateAndApplyOverallSpeed(TimelineContainer* self)
	{
		bool dirty = Hooks.CalculateAndApplyOverallSpeedHook.Original(self);

		int objectIndex = self->OwnerObject->ObjectIndex;
		AnimationController controller = this.GetController(objectIndex);
		dirty |= controller.CalculateAndApplyOverallSpeed();
		return dirty;
	}

	public partial class AnimationController(int objectIndex)
	{
		public readonly int ObjectIndex = objectIndex;

		[Notify] private float currentTime = 0;
		[Notify] private float duration = 0;
		[Notify] private float speed = 1.0f;

		public async Task PlayEmoteAsync(Emote emote)
		{
			await TickService.GameTick();
			this.PlayEmote(emote);
		}

		public unsafe void PlayEmote(Emote emote)
		{
			TickService.VerifyGameTickThread();

			Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectIndex);
			int poseKind = pCharacter->EmoteController.GetPoseKind();
			if (poseKind == -1)
				return;

			EmoteTimelineSlot timeLineSlot = GetTimelineSlotForPose((EmoteController.PoseType)poseKind);
			uint timelineId = emote.ActionTimeline[(int)timeLineSlot].RowId;
			if (timelineId == 0)
				timelineId = emote.ActionTimeline[(int)EmoteTimelineSlot.Standard].RowId;

			ActionTimeline? timeline = ServiceManager.Instance.GameData.GetRow<ActionTimeline>(timelineId);
			if (timeline == null)
				return;

			pCharacter->Timeline.TimelineSequencer.PlayTimeline((ushort)timelineId);
		}

		public unsafe void OnGameTick()
		{
			TickService.VerifyGameTickThread();

			Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectIndex);

			if (pCharacter == null)
				return;

			this.GetCurrentAnimationTime(pCharacter, out float time, out float duration);
			this.CurrentTime = time;
			this.Duration = duration;

			foreach(TimelineSlots slot in Enum.GetValues<TimelineSlots>())
			{
				ushort timelineId = pCharacter->Timeline.TimelineSequencer.GetSlotTimeline((uint)slot);
			}
		}

		public unsafe bool CalculateAndApplyOverallSpeed()
		{
			Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectIndex);
			float currentSpeed = pCharacter->Timeline.OverallSpeed;
			if (currentSpeed != this.speed)
			{
				pCharacter->Timeline.OverallSpeed = this.speed;
				return true;
			}

			return false;
		}

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

		private unsafe bool GetCurrentAnimationTime(Character* pCharacter, out float time, out float duration)
		{
			time = 0;
			duration = 0;

			if (pCharacter == null || pCharacter->GetCharacterBase() == null)
				return false;

			Skeleton* pSkeleton = pCharacter->GetCharacterBase()->Skeleton;
			for (int i = 0; i < pSkeleton->PartialSkeletonCount; i++)
			{
				hkaAnimatedSkeleton* pAnimatedSkeleton = pSkeleton->PartialSkeletons[i].GetHavokAnimatedSkeleton(0);
				if (pAnimatedSkeleton == null)
					continue;

				if (pAnimatedSkeleton->AnimationControls.Length <= 0)
					continue;

				hkaDefaultAnimationControl* pControl = pAnimatedSkeleton->AnimationControls[0].Value;
				if(pControl == null)
					continue;

				time = pControl->hkaAnimationControl.LocalTime;

				hkaAnimationBinding* binding = pControl->hkaAnimationControl.Binding.ptr;
				if(binding == null)
					continue;

				hkaAnimation* anim = binding->Animation.ptr;
				if(anim == null)
					continue;

				duration = anim->Duration;
				return true;
			}

			return false;
		}
	}
}
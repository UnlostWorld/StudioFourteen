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

using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using FFXIVClientStructs.Havok.Animation.Animation;
using FFXIVClientStructs.Havok.Animation.Playback;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using Serilog;
using StudioFourteen.Services;
using WpfUtils.Extensions;

[NotifyPropertyChanged]
public partial class AnimationController
{
	public readonly int ObjectIndex;

	protected readonly ILogger Log;

	private ITimelineAnimation? currentAnimation;
	private ushort? initialBaseOverride = 0;

	public AnimationController(int objectIndex)
	{
		this.ObjectIndex = objectIndex;
		this.Log = Logging.ForContext($"AnimationController {objectIndex}");
		this.Speed = 1.0f;
		this.CurrentBaseTimelineKey = string.Empty;
	}

	[Bind] public partial float CurrentTime { get; set; }
	[Bind] public partial float Duration { get; set; }
	[Bind] public partial float Speed { get; set; }
	[Bind] public partial bool EnableLoop { get; set; }
	[Bind] public partial string CurrentBaseTimelineKey { get; set; }
	[Bind] public partial bool IsPlayingAnimation { get; set; }

	public async Task PlayAnimationAsync(ITimelineAnimation emote)
	{
		await TickService.GameTick();
		this.PlayAnimation(emote);
	}

	public unsafe void PlayAnimation(ITimelineAnimation animation)
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = (Character*)ServiceManager.Instance.GameObjects.GetXivObject(this.ObjectIndex);
		if (pCharacter == null)
			return;

		this.currentAnimation = animation;

		if (this.EnableLoop)
		{
			if (this.initialBaseOverride == null)
				this.initialBaseOverride = pCharacter->Timeline.BaseOverride;

			pCharacter->Timeline.BaseOverride = animation.LoopTimelineId;
			this.PlayTimeline(pCharacter, animation.IntroTimelineId);
		}
		else if (animation.IntroTimelineId != 0)
		{
			this.PlayTimeline(pCharacter, animation.IntroTimelineId);
		}
		else
		{
			this.PlayTimeline(pCharacter, animation.LoopTimelineId);
		}
	}

	public async Task ResetLoopAsync()
	{
		await TickService.GameTick();
		this.ResetLoop();
	}

	public unsafe void ResetLoop()
	{
		TickService.VerifyGameTickThread();
		Character* pCharacter = (Character*)ServiceManager.Instance.GameObjects.GetXivObject(this.ObjectIndex);

		if (this.initialBaseOverride != null)
		{
			pCharacter->Timeline.BaseOverride = (ushort)this.initialBaseOverride;
			this.initialBaseOverride = null;
		}
		else
		{
			pCharacter->Timeline.BaseOverride = 0;
		}
	}

	public async Task ResetAsync()
	{
		await TickService.GameTick();
		this.Reset();
	}

	public unsafe void Reset()
	{
		TickService.VerifyGameTickThread();
		this.ResetLoop();

		Character* pCharacter = (Character*)ServiceManager.Instance.GameObjects.GetXivObject(this.ObjectIndex);
		this.PlayTimeline(pCharacter, 368);
	}

	public unsafe void OnGameTick()
	{
		TickService.VerifyGameTickThread();

		Character* pCharacter = (Character*)ServiceManager.Instance.GameObjects.GetXivObject(this.ObjectIndex);
		if (pCharacter == null)
			return;

		this.GetCurrentAnimationTime(pCharacter, out float liveTime, out float duration);
		this.Duration = duration;
		this.CurrentTime = liveTime;

		SchedulerTimeline* baseTimeline = pCharacter->Timeline.TimelineSequencer.GetSchedulerTimeline((int)AnimationService.TimelineSlots.Base);

		if (baseTimeline != null)
		{
			this.CurrentBaseTimelineKey = baseTimeline->ActionTimelineKey;
		}
		else
		{
			this.CurrentBaseTimelineKey = string.Empty;
		}

		if (this.currentAnimation != null)
		{
			ushort timeLineId = pCharacter->Timeline.TimelineSequencer.GetSlotTimeline((uint)AnimationService.TimelineSlots.Base);
			this.IsPlayingAnimation = this.currentAnimation.IntroTimelineId == timeLineId || this.currentAnimation.LoopTimelineId == timeLineId;

			if (!this.IsPlayingAnimation)
			{
				this.currentAnimation = null;
			}
		}
		else
		{
			this.IsPlayingAnimation = false;
		}
	}

	public unsafe bool CalculateAndApplyOverallSpeed(TimelineContainer* self)
	{
		float currentSpeed = self->OverallSpeed;

		if (currentSpeed != this.Speed)
		{
			self->OverallSpeed = this.Speed;
			return true;
		}

		return false;
	}

	protected void OnEnableLoopChanged(bool oldValue, bool newValue)
	{
		if (newValue == false)
		{
			this.ResetLoopAsync().Run();
		}
	}

	private unsafe void PlayTimeline(Character* pCharacter, ushort timelineId)
	{
		CharacterModes initialMode = pCharacter->Mode;
		byte initialModeParam = pCharacter->ModeParam;

		pCharacter->SetMode(CharacterModes.AnimLock, 0);
		pCharacter->Timeline.TimelineSequencer.PlayTimeline(timelineId);
		pCharacter->SetMode(initialMode, initialModeParam);
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
			if (pControl == null)
				continue;

			time = pControl->hkaAnimationControl.LocalTime;

			hkaAnimationBinding* binding = pControl->hkaAnimationControl.Binding.ptr;
			if (binding == null)
				continue;

			hkaAnimation* anim = binding->Animation.ptr;
			if (anim == null)
				continue;

			duration = anim->Duration;
			return true;
		}

		return false;
	}
}
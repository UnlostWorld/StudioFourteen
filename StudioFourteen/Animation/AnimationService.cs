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
using System.ComponentModel;
using System.Threading.Tasks;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.System.Scheduler.Base;
using FFXIVClientStructs.Havok.Animation.Animation;
using FFXIVClientStructs.Havok.Animation.Playback;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using PropertyChanged.SourceGenerator;
using Serilog;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using WpfUtils.Extensions;

public interface ITimelineAnimation
{
	public ushort LoopTimelineId { get; }
	public ushort IntroTimelineId { get; }
}

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
		try
		{
			AnimationController controller = this.GetController(objectIndex);
			dirty |= controller.CalculateAndApplyOverallSpeed(self);
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error in CalculateAndApplyOverallSpeed, hook will be disabled");
			Hooks.CalculateAndApplyOverallSpeedHook.Disable();
		}

		return dirty;
	}

	public partial class AnimationController : INotifyPropertyChanged
	{
		public readonly int ObjectIndex;

		protected readonly ILogger Log;

		[Notify] private float currentTime = 0;
		[Notify] private float duration = 0;
		[Notify] private float speed = 1.0f;
		[Notify] private bool enableLoop = false;
		[Notify] private string currentBaseTimelineKey = string.Empty;
		[Notify] private bool isPlayingAnimation = false;

		private ITimelineAnimation? currentAnimation;
		private ushort? initialBaseOverride = 0;

		public AnimationController(int objectIndex)
		{
			this.ObjectIndex = objectIndex;
			this.Log = Logging.ForContext($"AnimationController {objectIndex}");
		}

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

			if (this.enableLoop)
			{
				if (this.initialBaseOverride == null)
					this.initialBaseOverride = pCharacter->Timeline.BaseOverride;

				pCharacter->Timeline.BaseOverride =	animation.LoopTimelineId;
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
				pCharacter->Timeline.BaseOverride =	(ushort)this.initialBaseOverride;
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

			SchedulerTimeline* baseTimeline = pCharacter->Timeline.TimelineSequencer.GetSchedulerTimeline((int)TimelineSlots.Base);

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
				ushort timeLineId = pCharacter->Timeline.TimelineSequencer.GetSlotTimeline((uint)TimelineSlots.Base);
				this.IsPlayingAnimation = this.currentAnimation.IntroTimelineId == timeLineId || this.currentAnimation.LoopTimelineId == timeLineId;

				if (!this.IsPlayingAnimation)
				{
					this.currentAnimation = null;
				}
			}
		}

		public unsafe bool CalculateAndApplyOverallSpeed(TimelineContainer* self)
		{
			float currentSpeed = self->OverallSpeed;

			if (currentSpeed != this.speed)
			{
				self->OverallSpeed = this.speed;
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
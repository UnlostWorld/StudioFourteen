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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Interop;
using StudioFourteen.Services;

public interface ITimelineAnimation
{
	public ushort LoopTimelineId { get; }
	public ushort IntroTimelineId { get; }
}

[Service]
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
		foreach ((int objectIndex, AnimationController controller) in this.controllers)
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
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in CalculateAndApplyOverallSpeed, hook will be disabled");
			Hooks.CalculateAndApplyOverallSpeedHook.Disable();
		}

		return dirty;
	}
}
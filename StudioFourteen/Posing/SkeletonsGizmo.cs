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

namespace StudioFourteen.Posing;

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Rendering.Scene.Gizmos;
using StudioFourteen.Services;

public class SkeletonsGizmo : GizmoBase
{
	private readonly Dictionary<int, SkeletonGizmo> skeletonLookup = new();

	public override string Name => "Skeletons";
	public override bool KeepScreenSize => false;

	public override void Enable()
	{
		base.Enable();

		this.Services.CharacterLifecycle.CharacterCreated += this.OnCharacterCreated;
		this.Services.CharacterLifecycle.CharacterDestroyed += this.OnCharacterDestroyed;
		this.Services.Tick.Dispatch(TickService.Channels.GameTick, this.Initialize);
	}

	public override void Disable()
	{
		this.Services.CharacterLifecycle.CharacterCreated -= this.OnCharacterCreated;
		this.Services.CharacterLifecycle.CharacterDestroyed -= this.OnCharacterDestroyed;

		foreach((int objectTableIndex, SkeletonGizmo gizmo) in this.skeletonLookup)
		{
			this.Remove(gizmo);
			gizmo.Dispose();
		}

		this.skeletonLookup.Clear();

		base.Disable();
	}

	private unsafe void Initialize()
	{
		TickService.VerifyGameTickThread();

		Character*[] pCharacters = this.Services.CharacterLifecycle.GetAllCharacters();
		foreach(Character* pCharacter in pCharacters)
		{
			this.OnCharacterCreated(pCharacter->ObjectIndex);
		}
	}

	private void OnCharacterDestroyed(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		if (this.skeletonLookup.TryGetValue(objectTableIndex, out var skeletonGizmo))
		{
			skeletonGizmo.IsVisible = false;
			this.Remove(skeletonGizmo);
			skeletonGizmo.Dispose();
			this.skeletonLookup.Remove(objectTableIndex);
		}
	}

	private unsafe void OnCharacterCreated(int objectTableIndex)
	{
		TickService.VerifyGameTickThread();

		if (this.skeletonLookup.ContainsKey(objectTableIndex))
			return;

		SkeletonGizmo gizmo = new(objectTableIndex);
		this.Add(gizmo);
		this.skeletonLookup.Add(objectTableIndex, gizmo);
	}
}

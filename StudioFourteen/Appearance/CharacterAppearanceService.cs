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

namespace StudioFourteen.Appearance;

using StudioFourteen.Interop;
using StudioFourteen.Services;
using System.Threading.Tasks;

[Service]
public class CharacterAppearanceService : ServiceBase
{
	private readonly GroupPoseCharactersLibrarySource provider = new();

	public delegate void AppearanceChangedDelegate(int objectTableIndex);
	public event AppearanceChangedDelegate? OnAppearanceChanged;

	public override Task Start()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChange;
		this.Services.Library.AddSource(this.provider);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.provider.OnEnterGroupPose();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChange;
		return base.Stop();
	}

	public override void Attach()
	{
		base.Attach();

		Hooks.EnforceKind.Enable(this.EnforceKindRestrictionsDetour);
	}

	public override void Detach()
	{
		base.Detach();

		Hooks.EnforceKind.Disable();
	}

	public void RaiseAppearanceChanged(int objectTableIndex)
	{
		this.OnAppearanceChanged?.Invoke(objectTableIndex);
	}

	private void OnGroupPoseStateChange(bool newState)
	{
		if (newState)
		{
			this.provider.OnEnterGroupPose();
		}
	}

	private byte EnforceKindRestrictionsDetour(nint a1, nint a2)
	{
		// always allow npc values.
		////return this.enforceKindRestrictionsHook.Original(a1, a2);
		return 0;
	}
}
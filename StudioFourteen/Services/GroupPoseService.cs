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

namespace StudioFourteen.Services;

using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using StudioFourteen.Input.Devices;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;

using Task = System.Threading.Tasks.Task;

[Service]
public partial class GroupPoseService : ServiceBase
{
	public const int GPoseCharacterCount = 39;
	public const int GPoseFirstCharacter = 201;

	public delegate void OnStateChangedDelegate(bool newState);

	public event OnStateChangedDelegate? StateChanged;
	public event OnStateChangedDelegate? SettingsStateChanged;

	[Bind] public partial bool IsGroupPosing { get; private set; }
	[Bind] public partial bool IsGroupPoseSettingsWindowVisible { get; private set; }
	[Bind] public partial bool IsGroupPoseLoaded { get; private set; }

	public unsafe void SetGroupPose(bool state)
	{
		if (DalamudServices.GameGui == null)
			return;

		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			UIModule* pModule = (UIModule*)DalamudServices.GameGui.GetUIModule().Address;
			if (pModule != null)
			{
				if (state == pModule->IsInGPose())
					return;

				if (state)
				{
					pModule->EnterGPose();
				}
				else
				{
					pModule->ExitGPose();
				}
			}
		});
	}

	public override Task Initialize()
	{
		this.IsGroupPosing = DalamudServices.ClientState?.IsGPosing == true || this.Services.Territory.IsInTitleScreen;
		this.IsGroupPoseLoaded = this.IsGroupPosing;
		return base.Initialize();
	}

	public override unsafe void Attach()
	{
		base.Attach();

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		Hooks.EnterGroupPose.Enable(this.EnterDetour);
		Hooks.ExitGroupPose.Enable(this.ExitDetour);

		this.IsGroupPosing = DalamudServices.ClientState?.IsGPosing == true || this.Services.Territory.IsInTitleScreen;
		this.IsGroupPoseLoaded = this.IsGroupPosing;
	}

	public override void Detach()
	{
		base.Detach();

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		Hooks.EnterGroupPose.Disable();
		Hooks.ExitGroupPose.Disable();
	}

	public unsafe bool GetIsGroupPoseSettingsWindowVisible()
	{
		if (!DalamudServices.IsAlive)
			return false;

		AtkUnitBase* addon = RaptureAtkUnitManager.Instance()->GetAddonByName("CameraSetting");
		if (addon == null)
			return false;

		return addon->IsVisible;
	}

	public unsafe void SetGroupPoseSettingsWindowVisible(bool visible)
	{
		if (!DalamudServices.IsAlive)
			return;

		if (visible)
		{
			// Since the keybind for "auto-walk" can be changed (and often is), and
			// I have no idea how to open the settings window from the addon system,
			// just send the square button on the gamepad to open the addon.
			// this isn't ideal, as the addon will open with the gamepad controls visible,
			// but its better than having no way to open this at all.
			this.Services.Input.Gamepad?.SendButton(GamepadDevice.Buttons.FaceLeft);
		}
		else
		{
			AtkUnitBase* addon = RaptureAtkUnitManager.Instance()->GetAddonByName("CameraSetting");
			if (addon == null)
				return;

			addon->Hide2();
		}
	}

	protected void OnGameTick()
	{
		bool isWindowVisible = this.GetIsGroupPoseSettingsWindowVisible();
		if (isWindowVisible != this.IsGroupPoseSettingsWindowVisible)
		{
			this.IsGroupPoseSettingsWindowVisible = isWindowVisible;
			this.SettingsStateChanged?.Invoke(isWindowVisible);
		}
	}

	private unsafe bool EnterDetour(UIModule* uiModule)
	{
		bool didEnter = Hooks.EnterGroupPose.Original.Invoke(uiModule);

		if (didEnter)
			this.SetState(true);

		return didEnter;
	}

	private unsafe void ExitDetour(UIModule* uiModule)
	{
		Hooks.ExitGroupPose.Original.Invoke(uiModule);
		this.SetState(false);
	}

	private void SetState(bool newState)
	{
		this.Log.Information($"Group Pose State changed {newState}");
		this.StateChanged?.Invoke(newState);
		this.IsGroupPosing = newState;
		this.CheckLoaded().RunAsynchronously();
	}

	private async Task CheckLoaded()
	{
		this.IsGroupPoseLoaded = false;

		if (!this.IsGroupPosing)
			return;

		await Task.Delay(2000);
		this.IsGroupPoseLoaded = this.IsGroupPosing;
		this.Log.Information($"Group Pose Loaded");
	}
}

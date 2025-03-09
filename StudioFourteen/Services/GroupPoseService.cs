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

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Input.Devices;
using StudioFourteen.Plugin;
using Task = System.Threading.Tasks.Task;

public partial class GroupPoseService : ServiceBase
{
	public const int GPoseCharacterCount = 39;
	public const int GPoseFirstCharacter = 201;

	private Hook<EnterDelegate>? enterHook;
	private Hook<ExitDelegate>? exitHook;

	[Notify(Setter.Private)] private bool isGroupPosing;
	[Notify(Setter.Private)] private bool isGroupPoseSettingsWindowVisible;

	public delegate void OnStateChangedDelegate(bool newState);
	private unsafe delegate bool EnterDelegate(UIModule* uiModule);
	private unsafe delegate void ExitDelegate(UIModule* uiModule);

	public event OnStateChangedDelegate? StateChanged;
	public event OnStateChangedDelegate? SettingsStateChanged;

	public unsafe void SetGroupPose(bool state)
	{
		if (DalamudServices.GameGui == null)
			return;

		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			UIModule* pModule = (UIModule*)DalamudServices.GameGui.GetUIModule();
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
		this.IsGroupPosing = DalamudServices.ClientState?.IsGPosing == true || this.Services.Environment.IsInTitleScreen;
		return base.Initialize();
	}

	public override unsafe void Attach()
	{
		base.Attach();

		if (DalamudServices.Framework == null)
			return;

		UIModule* uiModule = Framework.Instance()->UIModule;
		var enterAddress = (nint)uiModule->VirtualTable->EnterGPose;
		var exitAddress = (nint)uiModule->VirtualTable->ExitGPose;

		this.enterHook = InteropService.HookFromAddress<EnterDelegate>(enterAddress, this.EnterDetour);
		this.enterHook?.Enable();

		this.exitHook = InteropService.HookFromAddress<ExitDelegate>(exitAddress, this.ExitDetour);
		this.exitHook?.Enable();

		this.IsGroupPosing = DalamudServices.ClientState?.IsGPosing == true || this.Services.Environment.IsInTitleScreen;
	}

	public override void Detach()
	{
		base.Detach();

		this.enterHook?.Dispose();
		this.exitHook?.Dispose();
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
			this.Services.Windows.ActivateXivWindow();
		}
		else
		{
			AtkUnitBase* addon = RaptureAtkUnitManager.Instance()->GetAddonByName("CameraSetting");
			if (addon == null)
				return;

			addon->Hide2();
		}
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		bool isWindowVisible = this.GetIsGroupPoseSettingsWindowVisible();
		if (isWindowVisible != this.isGroupPoseSettingsWindowVisible)
		{
			this.IsGroupPoseSettingsWindowVisible = isWindowVisible;
			this.SettingsStateChanged?.Invoke(isWindowVisible);
		}
	}

	private unsafe bool EnterDetour(UIModule* uiModule)
	{
		bool didEnter = this.enterHook?.Original.Invoke(uiModule) ?? false;

		if (didEnter)
			this.SetState(true);

		return didEnter;
	}

	private unsafe void ExitDetour(UIModule* uiModule)
	{
		this.exitHook?.Original.Invoke(uiModule);
		this.SetState(false);
	}

	private void SetState(bool newState)
	{
		this.StateChanged?.Invoke(newState);
		this.IsGroupPosing = newState;
		this.RaisePropertyChanged(nameof(GroupPoseService.IsGroupPosing));
	}
}

//// Brio
//// https://github.com/Etheirys/Brio/blob/main/Brio/Game/GPose/GPoseService.cs

namespace ScreenshotStudio.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using ScreenshotStudio.Plugin;

using Task = System.Threading.Tasks.Task;

public class GroupPoseService : ServiceBase
{
	public const int GPoseCharacterCount = 39;
	public const int GPoseFirstCharacter = 201;

	private Hook<EnterDelegate>? enterHook;
	private Hook<ExitDelegate>? exitHook;

	public delegate void OnStateChangedDelegate(bool newState);
	private unsafe delegate bool EnterDelegate(UIModule* uiModule);
	private unsafe delegate void ExitDelegate(UIModule* uiModule);

	public event OnStateChangedDelegate? StateChanged;

	public bool IsGroupPosing => DalamudServices.ClientState?.IsGPosing ?? false;

	public override Task Start()
	{
		this.Attach();
		return base.Start();
	}

	public override Task Stop()
	{
		this.Detach();
		return base.Stop();
	}

	private unsafe void Attach()
	{
		if (!DalamudServices.IsAlive)
			return;

		UIModule* uiModule = Framework.Instance()->UIModule;
		var enterAddress = (nint)uiModule->VirtualTable->EnterGPose;
		var exitAddress = (nint)uiModule->VirtualTable->ExitGPose;

		this.enterHook = InteropService.HookFromAddress<EnterDelegate>(enterAddress, this.EnterDetour);
		this.enterHook?.Enable();

		this.exitHook = InteropService.HookFromAddress<ExitDelegate>(exitAddress, this.ExitDetour);
		this.exitHook?.Enable();
	}

	private void Detach()
	{
		this.enterHook?.Dispose();
		this.exitHook?.Dispose();
	}

	private unsafe bool EnterDetour(UIModule* uiModule)
	{
		bool didEnter = this.enterHook?.Original.Invoke(uiModule) ?? false;

		if (didEnter)
		{
			this.StateChanged?.Invoke(true);
		}

		this.RaisePropertyChanged(nameof(GroupPoseService.IsGroupPosing));

		return didEnter;
	}

	private unsafe void ExitDetour(UIModule* uiModule)
	{
		this.exitHook?.Original.Invoke(uiModule);

		this.StateChanged?.Invoke(false);
		this.RaisePropertyChanged(nameof(GroupPoseService.IsGroupPosing));
	}
}

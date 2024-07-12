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
	public const int GPoseActorCount = 39;
	public const int GPoseFirstActor = 201;

	private Hook<EnterDelegate>? enterHook;
	private Hook<ExitDelegate>? exitHook;

	public delegate void OnStateChangedDelegate(bool newState);
	private unsafe delegate bool EnterDelegate(UIModule* uiModule);
	private unsafe delegate void ExitDelegate(UIModule* uiModule);

	public static event OnStateChangedDelegate? OnStateChange;

	public static bool IsGroupPosing => DalamudServices.ClientState?.IsGPosing ?? false;

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
			OnStateChange?.Invoke(true);
		}

		return didEnter;
	}

	private unsafe void ExitDetour(UIModule* uiModule)
	{
		this.exitHook?.Original.Invoke(uiModule);

		OnStateChange?.Invoke(false);
	}
}

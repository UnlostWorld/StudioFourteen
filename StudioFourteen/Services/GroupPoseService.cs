//// Brio
//// https://github.com/Etheirys/Brio/blob/main/Brio/Game/GPose/GPoseService.cs

namespace StudioFourteen.Services;

using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Common.Lua;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Plugin;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

public partial class GroupPoseService : ServiceBase
{
	public const int GPoseCharacterCount = 39;
	public const int GPoseFirstCharacter = 201;

	private Hook<EnterDelegate>? enterHook;
	private Hook<ExitDelegate>? exitHook;

	[Notify(Setter.Private)] private bool isGroupPosing;

	public delegate void OnStateChangedDelegate(bool newState);
	private unsafe delegate bool EnterDelegate(UIModule* uiModule);
	private unsafe delegate void ExitDelegate(UIModule* uiModule);

	public event OnStateChangedDelegate? StateChanged;

	public unsafe void SetGroupPose(bool state)
	{
		if (DalamudServices.GameGui == null)
			return;

		DalamudServices.Framework?.RunOnFrameworkThread(() =>
		{
			UIModule* pModule = (UIModule*)DalamudServices.GameGui.GetUIModule();
			if (pModule != null)
			{
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
		this.IsGroupPosing = DalamudServices.ClientState?.IsGPosing == true;
		return base.Initialize();
	}

	public override unsafe void Attach()
	{
		base.Attach();

		UIModule* uiModule = Framework.Instance()->UIModule;
		var enterAddress = (nint)uiModule->VirtualTable->EnterGPose;
		var exitAddress = (nint)uiModule->VirtualTable->ExitGPose;

		this.enterHook = InteropService.HookFromAddress<EnterDelegate>(enterAddress, this.EnterDetour);
		this.enterHook?.Enable();

		this.exitHook = InteropService.HookFromAddress<ExitDelegate>(exitAddress, this.ExitDetour);
		this.exitHook?.Enable();
	}

	public override void Detach()
	{
		base.Detach();

		this.enterHook?.Dispose();
		this.exitHook?.Dispose();
	}

	private unsafe bool EnterDetour(UIModule* uiModule)
	{
		bool didEnter = this.enterHook?.Original.Invoke(uiModule) ?? false;

		if (didEnter)
		{
			this.StateChanged?.Invoke(true);
			this.IsGroupPosing = true;
		}

		this.RaisePropertyChanged(nameof(GroupPoseService.IsGroupPosing));

		return didEnter;
	}

	private unsafe void ExitDetour(UIModule* uiModule)
	{
		this.exitHook?.Original.Invoke(uiModule);

		this.StateChanged?.Invoke(false);
		this.IsGroupPosing = false;
		this.RaisePropertyChanged(nameof(GroupPoseService.IsGroupPosing));
	}
}

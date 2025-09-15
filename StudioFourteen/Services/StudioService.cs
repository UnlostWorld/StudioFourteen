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

using PropertyChanged.SourceGenerator;
using StudioFourteen.Plugin;
using StudioFourteen.AIO;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using StudioFourteen.Input;
using WpfUtils.Extensions;
using StudioFourteen.GameData.Library;
using StudioFourteen.Appearance;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Controllers;

public partial class StudioService : ServiceBase
{
	private readonly Input0DListener hideUiListener = new(InputAction.HideUi);

	public delegate void OnStateChangedDelegate();

	public event OnStateChangedDelegate? Opening;
	public event OnStateChangedDelegate? Closing;

	[Bind] public partial bool IsOpen { get; set; }
	[Bind] public partial bool IsOpenAndInGPose { get; set; }
	[Bind] public partial bool HideUi { get; set; }

	public override async Task Initialize()
	{
		await base.Initialize();
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
	}

	public override Task Shutdown()
	{
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		return base.Shutdown();
	}

	public override Task Stop()
	{
		AioWindow.CloseAio();
		return base.Stop();
	}

	public override Task Start()
	{
		if (Debugger.IsAttached)
		{
			Task.Run(async () =>
			{
				while (ServiceManager.Instance.CurrentState <= ServiceManagerBase.States.Starting)
					await Task.Delay(10);

				this.OpenStudio();
			});
		}

		return base.Start();
	}

	public void OpenStudio()
	{
		if (this.IsOpen)
			return;

		try
		{
			this.Services.Attach();

			this.IsOpen = true;
			this.NotifyPropertyChanged(nameof(StudioService.IsOpen));
			this.NotifyPropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			this.Opening?.Invoke();

			this.IsOpen = true;
			this.IsOpenAndInGPose = this.Services.GroupPose.IsGroupPosing;

			if (this.Services.Settings.Current.OpenGroupPose)
			{
				this.Services.GroupPose.SetGroupPose(true);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error opening Studio Fourteen");
		}
	}

	public void CloseStudio()
	{
		if (!this.IsOpen)
			return;

		try
		{
			this.IsOpen = false;

			this.Services.Detach();
			this.NotifyPropertyChanged(nameof(StudioService.IsOpen));
			this.NotifyPropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			this.Closing?.Invoke();

			this.IsOpen = false;
			this.IsOpenAndInGPose = false;

			if (this.Services.Settings.Current.OpenGroupPose)
			{
				this.Services.GroupPose.SetGroupPose(false);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error closing Studio Fourteen");
		}
	}

	public unsafe override void Attach()
	{
		base.Attach();

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.hideUiListener.Enable();

		if (DalamudServices.GameGui == null)
			return;

		AtkManager.SetUnitVisibility("_TitleMenu", false);
		AtkManager.SetUnitVisibility("_TitleLogo", false);
		AtkManager.SetUnitVisibility("_TitleRevision", false);
		AtkManager.SetUnitVisibility("_TitleRights", false);

		this.SetupInitialScene().Run();
	}

	public unsafe override void Detach()
	{
		base.Detach();

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.hideUiListener.Disable();

		AtkManager.SetUnitVisibility("_TitleMenu", true);
		AtkManager.SetUnitVisibility("_TitleLogo", true);
		AtkManager.SetUnitVisibility("_TitleRevision", true);
		AtkManager.SetUnitVisibility("_TitleRights", true);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.IsOpenAndInGPose = this.IsOpen && newState;

		if (!this.IsOpen && this.Services.Settings.Current.OpenGroupPose)
		{
			this.OpenStudio();
		}
	}

	private void OnGameTick()
	{
		if (DalamudServices.GameGui == null)
			return;

		this.HideUi = this.hideUiListener.Value > 0.5f || DalamudServices.GameGui.GameUiHidden;
	}

	private async Task SetupInitialScene()
	{
		while (!this.Services.Territory.IsAttached)
			await Task.Delay(250);

		await TickService.GameTick();
		if (!this.Services.Territory.GetIsInTitleScreen())
			return;

		await TickService.GameTick();
		this.Services.Territory.ChangeTerritory("ffxiv/zon_z1/chr/z1c1/level/z1c1");
		await Task.Delay(100);

		// grab a random npc to spawn
		ICharacterAppearance? appearance = this.Services.GameData.GetLibraryEntry<ENpcResidentLibraryEntry>(1039305);
		if (appearance == null)
			return;

		Character? character = await this.Services.CharacterLifecycle.CreateAsync(appearance, UpdateSource.Script);

		if (character == null)
			return;

		this.Services.Selection.Select(character, this);
		CharacterController controller = character.SetController<CharacterController>();
		controller.Activate();
	}
}
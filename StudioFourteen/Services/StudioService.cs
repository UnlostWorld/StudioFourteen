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
using StudioFourteen.SPA;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

public partial class StudioService : ServiceBase
{
	[Notify] private bool isOpen;
	[Notify] private bool isOpenAndInGPose;

	public delegate void OnStateChangedDelegate();

	public event OnStateChangedDelegate? Opening;
	public event OnStateChangedDelegate? Closing;

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

	public override async Task Start()
	{
		await base.Start();

		#if DEBUG
		{
			if (this.Settings.WasStudioOpen)
			{
				_ = Task.Run(async () =>
				{
					await Task.Delay(2000);
					this.Log.Information("Restoring studio state");
					this.OpenStudio();
				});
			}
		}
		#endif
	}

	public override Task Stop()
	{
		this.Settings.WasStudioOpen = this.isOpen;
		SpaWindow.CloseSpa();
		return base.Stop();
	}

	public void OpenStudio()
	{
		if (this.IsOpen)
			return;

		try
		{
			this.Services.Attach();

			this.IsOpen = true;
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			this.Opening?.Invoke();

			this.IsOpen = true;
			this.IsOpenAndInGPose = this.Services.GroupPose.IsGroupPosing;

			if (this.Services.Settings.Current.OpenGroupPose)
			{
				this.Services.GroupPose.SetGroupPose(true);
			}
		}
		catch(Exception ex)
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
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			this.Closing?.Invoke();

			this.IsOpen = false;
			this.IsOpenAndInGPose = false;

			if (this.Services.Settings.Current.OpenGroupPose)
			{
				this.Services.GroupPose.SetGroupPose(false);
			}
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Studio Fourteen");
		}
	}

	public unsafe override void Attach()
	{
		base.Attach();

		if (DalamudServices.GameGui == null)
			return;

		AtkManager.SetUnitVisibility("_TitleMenu", false);
		AtkManager.SetUnitVisibility("_TitleLogo", false);
		AtkManager.SetUnitVisibility("_TitleRevision", false);
		AtkManager.SetUnitVisibility("_TitleRights", false);
	}

	public unsafe override void Detach()
	{
		base.Detach();

		AtkManager.SetUnitVisibility("_TitleMenu", true);
		AtkManager.SetUnitVisibility("_TitleLogo", true);
		AtkManager.SetUnitVisibility("_TitleRevision", true);
		AtkManager.SetUnitVisibility("_TitleRights", true);
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.IsOpenAndInGPose = this.isOpen && newState;

		if (!this.IsOpen && this.Services.Settings.Current.OpenGroupPose)
		{
			this.OpenStudio();
		}
	}
}
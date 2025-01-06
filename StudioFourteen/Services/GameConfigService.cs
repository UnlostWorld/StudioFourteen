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

using Dalamud.Game.Config;
using Dalamud.Plugin.Services;
using StudioFourteen.Plugin;

public class GameConfigService : ServiceBase
{
	private bool userIsSoundAlways;
	private bool userIsSoundBgmAlways;
	private bool userIsSoundSeAlways;
	private bool userIsSoundVoiceAlways;
	private bool userIsSoundSystemAlways;
	private bool userIsSoundEnvAlways;
	private bool userIsSoundPerformAlways;
	private bool userFpsInactive;

	private bool hasChangedSettings = false;
	private bool hasBackedUpSettings = false;

	public override void Attach()
	{
		if (DalamudServices.GameConfig == null)
			return;

		this.hasBackedUpSettings = true;

		// Backup the users "Play sounds when window is not active" settings
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundAlways, out this.userIsSoundAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundBgmAlways, out this.userIsSoundBgmAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundSeAlways, out this.userIsSoundSeAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundVoiceAlways, out this.userIsSoundVoiceAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundSystemAlways, out this.userIsSoundSystemAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundEnvAlways, out this.userIsSoundEnvAlways);
		DalamudServices.GameConfig.TryGet(SystemConfigOption.IsSoundPerformAlways, out this.userIsSoundPerformAlways);

		// Backup the users "Limit frame rate when client is inactive" setting
		DalamudServices.GameConfig.TryGet(SystemConfigOption.FPSInActive, out this.userFpsInactive);

		base.Attach();
	}

	public override void Detach()
	{
		this.RestoreFocusLostSettings();
		this.hasBackedUpSettings = false;
		base.Detach();
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (!this.IsAttached)
			return;

		if (!this.hasBackedUpSettings)
			return;

		if (this.Services.Windows.IsAnyStudioWindowActive() && !this.hasChangedSettings)
		{
			// We have focus, disable settings.
			this.DisableFocusLostSettings();
		}
		else if (this.hasChangedSettings && !this.Services.Windows.IsAnyStudioWindowActive() && this.Services.Windows.IsAnyWindowActive())
		{
			// we do not have focus, restore settings.
			this.RestoreFocusLostSettings();
		}
	}

	private void DisableFocusLostSettings()
	{
		if (!this.IsAttached)
			return;

		if (DalamudServices.GameConfig == null)
			return;

		this.hasChangedSettings = true;

		// Change all settings so users cant tell that xiv has lost focus.
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundBgmAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundSeAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundVoiceAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundSystemAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundEnvAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundPerformAlways, true);
		DalamudServices.GameConfig.Set(SystemConfigOption.FPSInActive, false);
	}

	private void RestoreFocusLostSettings()
	{
		if (!this.IsAttached)
			return;

		if (DalamudServices.GameConfig == null)
			return;

		this.hasChangedSettings = false;

		// Restore the users settings
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundAlways, this.userIsSoundAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundBgmAlways, this.userIsSoundBgmAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundSeAlways, this.userIsSoundSeAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundVoiceAlways, this.userIsSoundVoiceAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundSystemAlways, this.userIsSoundSystemAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundEnvAlways, this.userIsSoundEnvAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.IsSoundPerformAlways, this.userIsSoundPerformAlways);
		DalamudServices.GameConfig.Set(SystemConfigOption.FPSInActive, this.userFpsInactive);
	}
}

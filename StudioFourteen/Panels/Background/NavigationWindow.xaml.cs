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

namespace StudioFourteen.Studio.Background;

using StudioFourteen.Appearance;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Posing;
using StudioFourteen.Save;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using StudioFourteen.SPA;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

public partial class NavigationWindow : PanelWindow
{
	private readonly Persistence persistence = new("NavigationWindow");
	private bool spa = false;

	[AutoNotify]
	public bool ShowStudioButton
	{
		get
		{
			if (!DalamudServices.IsAlive
				|| DalamudServices.GameGui == null)
				return true;

			if (this.Services.Settings.Current.HideStudioButton)
				return false;

			if (DalamudServices.GameGui.GameUiHidden)
				return false;

			return true;
		}
	}

	[AutoNotify]
	public bool IsFullyLoaded
	{
		get
		{
			return this.Services.CurrentState == ServiceManagerBase.States.Started;
		}
	}

	[AutoNotify]
	public unsafe bool IsInGPose
	{
		get => this.Services.GroupPose.IsGroupPosing;
		set => this.Services.GroupPose.SetGroupPose(value);
	}

	[AutoNotify]
	public unsafe bool IsGPoseSettingsOpen
	{
		get => this.Services.GroupPose.IsGroupPoseSettingsWindowVisible();
		set => this.Services.GroupPose.SetGroupPoseSettingsWindowVisible(value);
	}

	[AutoNotify]
	public bool IsLibraryOpen
	{
		get => this.Services.Panels.GetIsOpen<LibraryWindow>();
		set => this.Services.Panels.SetIsOpen<LibraryWindow>(true);
	}

	[AutoNotify]
	public bool IsCameraOpen
	{
		get => this.Services.Panels.GetIsOpen<CameraPanel>();
		set => this.Services.Panels.SetIsOpen<CameraPanel>(true);
	}

	[AutoNotify]
	public bool IsEnvironmentOpen
	{
		get => this.Services.Panels.GetIsOpen<EnvironmentPanel>();
		set => this.Services.Panels.SetIsOpen<EnvironmentPanel>(true);
	}

	[AutoNotify]
	public bool IsCharacterOpen
	{
		get => this.Services.Panels.GetIsOpen<CharacterPanel>();
		set => this.Services.Panels.SetIsOpen<CharacterPanel>(true);
	}

	[AutoNotify]
	public bool IsPoseOpen
	{
		get => this.Services.Panels.GetIsOpen<PoseWindow>();
		set => this.Services.Panels.SetIsOpen<PoseWindow>(true);
	}

	[AutoNotify]
	public bool IsSettingsOpen
	{
		get => this.Services.Panels.GetIsOpen<SettingsPanel>();
		set => this.Services.Panels.SetIsOpen<SettingsPanel>(true);
	}

	[AutoNotify]
	public bool IsPhotoOpen
	{
		get => this.Services.Panels.GetIsOpen<PhotoWindow>();
		set => this.Services.Panels.SetIsOpen<PhotoWindow>(true);
	}

	[AutoNotify]
	public bool IsSaveOpen
	{
		get => this.Services.Panels.GetIsOpen<SaveWindow>();
		set => this.Services.Panels.SetIsOpen<SaveWindow>(true);
	}

	[AutoNotify]
	public bool IsSpaOpen
	{
		get => this.spa;
		set
		{
			this.spa = value;
			if (value)
			{
				SpaWindow.OpenSpa();
			}
			else
			{
				SpaWindow.CloseSpa();
			}
		}
	}

	public override T? GetPersistence<T>([CallerMemberName] string id = "")
		where T : default
	{
		return this.persistence.GetPersistence<T>(id);
	}

	public override void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.persistence.SetPersistence(value, id);
	}

	public override void SetPersistence(string id, object? value)
	{
		this.persistence.SetPersistence(id, value);
	}

	private void OnStudioClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.Services.Studio.CloseStudio();
		}
		else
		{
			this.Services.Studio.OpenStudio();
		}
	}

	private void OnTitleMouseDown(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed)
		{
			this.DragMove();
		}
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		this.SavedPosition = this.Position;
	}
}
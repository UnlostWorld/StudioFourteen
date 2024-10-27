namespace StudioFourteen.Studio.Background;

using FFXIVClientStructs.FFXIV.Client.UI;
using StudioFourteen.Appearance;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Panels;
using StudioFourteen.Plugin;
using StudioFourteen.Posing;
using StudioFourteen.Save;
using StudioFourteen.Settings;
using System.Windows;
using System.Windows.Controls.Primitives;

public partial class Navigation : View
{
	public event DragDeltaEventHandler? DragDelta;

	[AutoNotify]
	public bool ShowNavigation => this.Services.Studio.IsOpen && !this.Services.Settings.Current.IsSpa;

	[AutoNotify]
	public unsafe bool IsInGPose
	{
		get => this.Services.GroupPose.IsGroupPosing;
		set
		{
			if (DalamudServices.GameGui == null)
				return;

			DalamudServices.Framework?.RunOnFrameworkThread(() =>
			{
				UIModule* pModule = (UIModule*)DalamudServices.GameGui.GetUIModule();
				if (pModule != null)
				{
					if (value)
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
	}

	[AutoNotify]
	public bool IsLibraryOpen
	{
		get => this.Services.Panels.GetIsOpen<LibraryWindow>();
		set => this.Services.Panels.SetIsOpen<LibraryWindow>(value);
	}

	[AutoNotify]
	public bool IsCameraOpen
	{
		get => this.Services.Panels.GetIsOpen<CameraPanel>();
		set => this.Services.Panels.SetIsOpen<CameraPanel>(value);
	}

	[AutoNotify]
	public bool IsEnvironmentOpen
	{
		get => this.Services.Panels.GetIsOpen<HelloWorldWindow>();
		set => this.Services.Panels.SetIsOpen<HelloWorldWindow>(value);
	}

	[AutoNotify]
	public bool IsCharacterOpen
	{
		get => this.Services.Panels.GetIsOpen<CharacterPanel>();
		set => this.Services.Panels.SetIsOpen<CharacterPanel>(value);
	}

	[AutoNotify]
	public bool IsPoseOpen
	{
		get => this.Services.Panels.GetIsOpen<PoseWindow>();
		set => this.Services.Panels.SetIsOpen<PoseWindow>(value);
	}

	[AutoNotify]
	public bool IsSettingsOpen
	{
		get => this.Services.Panels.GetIsOpen<SettingsPanel>();
		set => this.Services.Panels.SetIsOpen<SettingsPanel>(value);
	}

	[AutoNotify]
	public bool IsPhotoOpen
	{
		get => this.Services.Panels.GetIsOpen<PhotoWindow>();
		set => this.Services.Panels.SetIsOpen<PhotoWindow>(value);
	}

	[AutoNotify]
	public bool IsSaveOpen
	{
		get => this.Services.Panels.GetIsOpen<SaveWindow>();
		set => this.Services.Panels.SetIsOpen<SaveWindow>(value);
	}

	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.Services.Input.AddListener(Input.KeyBindEvents.SaveAs, this.OnToggleSave);
	}

	protected override void OnUnloaded()
	{
		base.OnUnloaded();
		this.Services.Input.AddListener(Input.KeyBindEvents.SaveAs, this.OnToggleSave);
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

	private void OnToggleSave()
	{
		this.IsSaveOpen = !this.IsSaveOpen;
	}

	private void OnDragDelta(object sender, DragDeltaEventArgs e)
	{
		this.DragDelta?.Invoke(this, e);
	}
}
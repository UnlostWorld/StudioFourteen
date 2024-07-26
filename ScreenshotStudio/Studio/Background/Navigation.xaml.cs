namespace ScreenshotStudio.Studio.Background;

using FFXIVClientStructs.FFXIV.Client.UI;
using ScreenshotStudio.Library;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Services;

public partial class Navigation : View
{
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
	public bool IsZoneOpen
	{
		get => this.Services.Panels.GetIsOpen<ZoneWindow>();
		set => this.Services.Panels.SetIsOpen<ZoneWindow>(value);
	}

	[AutoNotify]
	public bool IsCameraOpen
	{
		get => this.Services.Panels.GetIsOpen<HelloWorldWindow>();
		set => this.Services.Panels.SetIsOpen<HelloWorldWindow>(value);
	}

	[AutoNotify]
	public bool IsCustomizeOpen
	{
		get => this.Services.Panels.GetIsOpen<CustomizeWindow>();
		set => this.Services.Panels.SetIsOpen<CustomizeWindow>(value);
	}

	[AutoNotify]
	public bool IsGearOpen
	{
		get => this.Services.Panels.GetIsOpen<GearWindow>();
		set => this.Services.Panels.SetIsOpen<GearWindow>(value);
	}

	[AutoNotify]
	public bool IsShadersOpen
	{
		get => this.Services.Panels.GetIsOpen<HelloWorldWindow>();
		set => this.Services.Panels.SetIsOpen<HelloWorldWindow>(value);
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
		get => this.Services.Panels.GetIsOpen<HelloWorldWindow>();
		set => this.Services.Panels.SetIsOpen<HelloWorldWindow>(value);
	}

	[AutoNotify]
	public bool IsPhotoOpen
	{
		get => this.Services.Panels.GetIsOpen<PhotoWindow>();
		set => this.Services.Panels.SetIsOpen<PhotoWindow>(value);
	}
}
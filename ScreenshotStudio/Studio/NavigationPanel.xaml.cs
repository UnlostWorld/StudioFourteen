namespace ScreenshotStudio.Studio;

using FFXIVClientStructs.FFXIV.Client.UI;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Services;
using ScreenshotStudio.Windows;
using System.Windows;

public partial class NavigationPanel : DockPanel
{
	[AutoNotify] public bool IsExpanded { get; set; } = false;

	[AutoNotify]
	public unsafe bool IsInGPose
	{
		get => DalamudServices.ClientState.IsGPosing;
		set
		{
			DalamudServices.Framework.RunOnFrameworkThread(() =>
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
		get => this.Services.Panels.GetIsOpen<HelloWorldWindow>();
		set => this.Services.Panels.SetIsOpen<HelloWorldWindow>(value);
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

	public void Expand()
	{
		this.IsExpanded = true;
	}

	public void Collapse()
	{
		this.IsExpanded = false;
	}

	private void OnStudioClicked(object sender, RoutedEventArgs e)
	{
		if (this.IsExpanded)
		{
			this.Services.Studio.CloseStudio();
		}
		else
		{
			this.Services.Studio.OpenStudio();
		}
	}
}

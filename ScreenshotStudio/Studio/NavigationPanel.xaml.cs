// © XivTools.
// Licensed under the MIT license.

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
		get => DalamudServices.PluginInterface.UiBuilder.GposeActive;
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

	private void OnGearClicked(object sender, RoutedEventArgs e) => Panel.Show<GearWindow>();
	private void OnCustomizeClicked(object sender, RoutedEventArgs e) => Panel.Show<CustomizeWindow>();
	private void OnZoneClicked(object sender, RoutedEventArgs e) => Panel.Show<ZoneWindow>();
}

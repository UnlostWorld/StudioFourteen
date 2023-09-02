// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using ScreenshotStudio.Plugin;
using ScreenshotStudio.Studio;
using ScreenshotStudio.Windows;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;

public class StudioService : ServiceBase
{
	private StudioButtonWindow? buttonWindow;
	private NavigationPanel? navigationPanel;
	private TargetPanel? targetPanel;

	public override async Task Start()
	{
		await base.Start();

		this.buttonWindow = await Panel.ShowAsync<StudioButtonWindow>();
	}

	public void OpenStudio() => Task.Run(async () => await this.OpenStudioAsync());
	public void CloseStudio() => Task.Run(async () => await this.CloseStudioAsync());

	public async Task OpenStudioAsync()
	{
		try
		{
			if (this.buttonWindow != null)
				await this.buttonWindow.CloseAsync();

			this.navigationPanel = await Panel.ShowAsync<NavigationPanel>();
			this.targetPanel = await Panel.ShowAsync<TargetPanel>();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error opening Screenshot Studio");
		}
	}

	public async Task CloseStudioAsync()
	{
		try
		{
			if (this.navigationPanel != null)
				await this.navigationPanel.CloseAsync();

			if (this.targetPanel != null)
				await this.targetPanel.CloseAsync();

			this.buttonWindow = await Panel.ShowAsync<StudioButtonWindow>();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Screenshot Studio");
		}
	}
}
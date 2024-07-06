namespace ScreenshotStudio.Services;

using ScreenshotStudio.Studio;
using ScreenshotStudio.Windows;
using System.Threading.Tasks;
using System;
using ScreenshotStudio.Plugin;

public class StudioService : ServiceBase
{
	private NavigationPanel? navigationPanel;
	private TargetPanel? targetPanel;

	public bool IsOpen { get; private set; }

	public bool IsOpenAndInGPose
	{
		get
		{
			if (!this.Services.Studio.IsOpen)
				return false;

			return GroupPoseService.IsGroupPosing;
		}
	}

	public override async Task Start()
	{
		await base.Start();

		this.navigationPanel = await Panel.ShowAsync<NavigationPanel>();
		this.targetPanel = await Panel.ShowAsync<TargetPanel>();
	}

	public void OpenStudio() => Task.Run(async () => await this.OpenStudioAsync());
	public void CloseStudio() => Task.Run(async () => await this.CloseStudioAsync());

	public Task OpenStudioAsync()
	{
		try
		{
			this.IsOpen = true;
			this.navigationPanel?.Expand();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error opening Screenshot Studio");
		}

		return Task.CompletedTask;
	}

	public Task CloseStudioAsync()
	{
		try
		{
			this.IsOpen = false;
			this.navigationPanel?.Collapse();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Screenshot Studio");
		}

		return Task.CompletedTask;
	}
}
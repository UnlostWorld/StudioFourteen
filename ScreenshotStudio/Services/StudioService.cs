namespace ScreenshotStudio.Services;

using System.Threading.Tasks;
using System;

public class StudioService : ServiceBase
{
	public bool IsOpen { get; private set; }
	public bool IsOpenAndInGPose => this.Services.Studio.IsOpen && this.Services.GroupPose.IsGroupPosing;

	public override async Task Start()
	{
		await base.Start();

		if (this.Services.Settings.Current.IsOpen)
		{
			this.OpenStudio();
		}
	}

	public void OpenStudio() => Task.Run(async () => await this.OpenStudioAsync());
	public void CloseStudio() => Task.Run(async () => await this.CloseStudioAsync());

	public Task OpenStudioAsync()
	{
		try
		{
			this.Services.Settings.Current.IsOpen = true;
			this.IsOpen = true;
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));
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
			this.Services.Settings.Current.IsOpen = false;
			this.IsOpen = false;
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Screenshot Studio");
		}

		return Task.CompletedTask;
	}
}
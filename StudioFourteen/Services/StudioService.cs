namespace StudioFourteen.Services;

using System.Threading.Tasks;
using System;
using StudioFourteen.SPA;

public class StudioService : ServiceBase
{
	public delegate void OnStateChangedDelegate();

	public event OnStateChangedDelegate? Opening;
	public event OnStateChangedDelegate? Closing;

	public bool IsOpen { get; private set; }
	public bool IsOpenAndInGPose => this.Services.Studio.IsOpen && this.Services.GroupPose.IsGroupPosing;

	public override Task Stop()
	{
		SpaWindow.CloseSpa();
		return base.Stop();
	}

	public void OpenStudio() => Task.Run(async () => await this.OpenStudioAsync());
	public void CloseStudio() => Task.Run(async () => await this.CloseStudioAsync());

	public Task OpenStudioAsync()
	{
		try
		{
			this.Services.Attach();

			this.IsOpen = true;
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			if (this.Services.Settings.Current.IsSpa)
			{
				SpaWindow.OpenSpa();
			}

			this.Opening?.Invoke();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error opening Studio Fourteen");
		}

		return Task.CompletedTask;
	}

	public Task CloseStudioAsync()
	{
		try
		{
			this.Services.Detach();

			this.IsOpen = false;
			this.RaisePropertyChanged(nameof(StudioService.IsOpen));
			this.RaisePropertyChanged(nameof(StudioService.IsOpenAndInGPose));

			if (this.Services.Settings.Current.IsSpa)
			{
				SpaWindow.CloseSpa();
			}

			this.Closing?.Invoke();
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Studio Fourteen");
		}

		return Task.CompletedTask;
	}
}
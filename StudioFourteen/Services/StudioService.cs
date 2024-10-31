namespace StudioFourteen.Services;

using System.Threading.Tasks;
using System;
using StudioFourteen.SPA;
using PropertyChanged.SourceGenerator;

public partial class StudioService : ServiceBase
{
	[Notify] private bool isOpen;
	[Notify] private bool isOpenAndInGPose;

	public delegate void OnStateChangedDelegate();

	public event OnStateChangedDelegate? Opening;
	public event OnStateChangedDelegate? Closing;

	public override Task Initialize()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		return base.Initialize();
	}

	public override Task Shutdown()
	{
		this.Services.GroupPose.StateChanged -= this.OnGroupPoseStateChanged;
		return base.Shutdown();
	}

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

			this.IsOpen = true;
			this.IsOpenAndInGPose = this.Services.GroupPose.IsGroupPosing;
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

			this.IsOpen = false;
			this.IsOpenAndInGPose = false;
		}
		catch(Exception ex)
		{
			this.Log.Error(ex, "Error closing Studio Fourteen");
		}

		return Task.CompletedTask;
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.IsOpenAndInGPose = this.isOpen && newState;
	}
}
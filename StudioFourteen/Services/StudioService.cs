namespace StudioFourteen.Services;

using PropertyChanged.SourceGenerator;
using StudioFourteen.SPA;
using System;
using System.Threading.Tasks;

public partial class StudioService : ServiceBase
{
	[Notify] private bool isOpen;
	[Notify] private bool isOpenAndInGPose;

	public delegate void OnStateChangedDelegate();

	public event OnStateChangedDelegate? Opening;
	public event OnStateChangedDelegate? Closing;

	public override async Task Initialize()
	{
		await base.Initialize();
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
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

	public void OpenStudio()
	{
		if (this.IsOpen)
			return;

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
	}

	public void CloseStudio()
	{
		if (!this.IsOpen)
			return;

		try
		{
			this.IsOpen = false;

			this.Services.Detach();
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
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		this.IsOpenAndInGPose = this.isOpen && newState;
	}
}
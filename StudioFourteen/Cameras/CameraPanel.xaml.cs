namespace StudioFourteen.Panels;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Cameras;
using System.Collections.ObjectModel;
using System.Windows;
using WpfUtils.Extensions;

public partial class CameraPanel : Panel
{
	[Notify] private StudioCameraBase? current;
	[Notify] private FastObservableCollection<StudioCameraBase> cameras = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Camera.CamerasChanged += this.OnCamerasChanged;
		this.Services.Camera.CurrentCameraChanged += this.OnCurrentCameraChanged;
		this.OnCamerasChanged();
	}

	private void OnCreateOrbitTargetCameraClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Camera.CreateCamera<OrbitTargetCamera>();
		this.AddCameraButton.IsChecked = false;
	}

	private void OnCreateOrbitCameraClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Camera.CreateCamera<OrbitCamera>();
		this.AddCameraButton.IsChecked = false;
	}

	private void OnCreateFreeCameraClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Camera.CreateCamera<FreeCamera>();
		this.AddCameraButton.IsChecked = false;
	}

	private void OnDeleteCameraClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Camera.Current == null)
			return;

		this.Services.Camera.DeleteCamera(this.Services.Camera.Current);
	}

	private void OnResetCameraClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Camera.Current == null)
			return;

		this.Services.Camera.Current.Reset();
	}

	private void OnCamerasChanged()
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Cameras.Replace(this.Services.Camera.Cameras);
		});
	}

	private void OnCurrentCameraChanged(StudioCameraBase? oldCamera, StudioCameraBase? newCamera)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Current = this.Services.Camera.Current;
		});
	}

	private void OnCurrentChanged()
	{
		if (this.current == null)
			return;

		this.Services.Camera.Current = this.current;
	}
}
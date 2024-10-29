namespace StudioFourteen.Panels;

using StudioFourteen.Cameras;
using System.Collections.ObjectModel;
using System.Windows;
using WpfUtils.Extensions;

public partial class CameraPanel : Panel
{
	public FastObservableCollection<StudioCameraBase> Cameras { get; init; } = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Camera.CamerasChanged += this.OnCamerasChanged;
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

	private void OnCamerasChanged()
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Cameras.Replace(this.Services.Camera.Cameras);
		});
	}
}
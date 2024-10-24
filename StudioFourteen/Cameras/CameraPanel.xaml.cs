namespace StudioFourteen.Panels;

using StudioFourteen.Cameras;
using System.Windows;

public partial class CameraPanel : Panel
{
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
}
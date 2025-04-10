// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Cameras.Modifiers;
using StudioFourteen.Panels;
using System.Collections.Generic;
using System.Windows;
using WpfUtils.Extensions;

public partial class CameraPanel : Panel
{
	[Notify] private TabEntryBase? current;
	[Notify] private FastObservableCollection<TabEntryBase> tabs = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Camera.CamerasChanged += this.OnCamerasChanged;
		this.Services.Camera.CurrentCameraChanged += this.OnCurrentCameraChanged;
		this.PopulateTabs();
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

	private void OnCreateShakyModifieraClicked(object sender, RoutedEventArgs e)
	{
		this.Services.Camera.Current?.Modifiers.Add(new ShakyModifier());
		this.AddCameraButton.IsChecked = false;
		this.PopulateTabs();
	}

	private void OnActivateCameraClicked(object sender, RoutedEventArgs e)
	{
		if (this.Current is CameraEntry camera)
		{
			this.Services.Camera.Current = camera.Camera;
		}
		else if (this.current is ModifierEntry modifier)
		{
			this.Services.Camera.Current = modifier.Parent?.Camera;
		}
	}

	private void OnDeleteCameraClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Camera.Current == null)
			return;

		if (this.current is CameraEntry cameraEntry && cameraEntry.Camera != null)
		{
			this.Services.Camera.DeleteCamera(cameraEntry.Camera);
		}
		else if (this.current is ModifierEntry modifierEntry && modifierEntry.Modifier != null)
		{
			modifierEntry.Parent?.Camera?.Modifiers.Remove(modifierEntry.Modifier);
		}

		this.PopulateTabs();
	}

	private void OnResetCameraClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Camera.Current == null)
			return;

		this.Services.Camera.Current.Reset();
	}

	private void OnCamerasChanged()
	{
		this.Dispatcher.Invoke(this.PopulateTabs);
	}

	private void OnCurrentCameraChanged(StudioCameraBase? oldCamera, StudioCameraBase? newCamera)
	{
		/*this.Dispatcher.Invoke(() =>
		{
			this.Current = this.Services.Camera.Current;
		});*/
	}

	private void OnCurrentChanged()
	{
		if (this.current is CameraEntry camera)
		{
			this.Services.Camera.Current = camera.Camera;
		}
	}

	private void PopulateTabs()
	{
		List<TabEntryBase> newEntries = new();
		foreach(StudioCameraBase camera in this.Services.Camera.Cameras)
		{
			CameraEntry camEntry = new(camera);
			newEntries.Add(camEntry);

			foreach(CameraModifierBase modifier in camera.Modifiers)
			{
				newEntries.Add(new ModifierEntry(camEntry, modifier));
			}
		}

		this.Tabs.Replace(newEntries);

		if (newEntries.Count <= 0)
			return;

		this.Current = newEntries[0];
	}
}

public abstract class TabEntryBase
{
}

public class CameraEntry(StudioCameraBase camera)
	: TabEntryBase
{
	public StudioCameraBase? Camera => camera;
}

public class ModifierEntry(CameraEntry parent, CameraModifierBase modifier)
	: TabEntryBase
{
	public CameraEntry? Parent => parent;
	public CameraModifierBase? Modifier => modifier;
}
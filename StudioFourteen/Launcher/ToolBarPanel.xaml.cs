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

namespace StudioFourteen.Launcher;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Panels;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Scene;
using WpfUtils.Extensions;

public partial class ToolBarPanel : Panel
{
	[Notify] private bool allowMouseCapture = true;

	public FastObservableCollection<GizmoGroup> Gizmos { get; init; } = new();

	protected override void OnOpened()
	{
		base.OnOpened();

		this.Services.Settings.SettingChanged += this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.AllowMouseCapture = this.Settings.AllowMouseCapture;

		this.OnSelectionChanged(null, this.Services.Selection.Current, null);
	}

	protected override void OnClosed()
	{
		base.OnClosed();

		this.Services.Settings.SettingChanged -= this.OnSettingsOpenChanged;
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? source)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.Gizmos.Clear();
			this.Gizmos.Add(this.Services.Gizmos.Transform);

			if (newSelection != null)
			{
				foreach (GizmoBase gizmo in newSelection.Gizmos)
				{
					if (gizmo is GizmoGroup group)
					{
						this.Gizmos.Add(group);
					}
				}
			}
		});
	}

	private void OnAllowMouseCaptureChanged(bool oldValue, bool newValue)
	{
		this.Settings.AllowMouseCapture = newValue;
	}

	private void OnSettingsOpenChanged(string settingName, object? newValue)
	{
		this.Dispatcher.Invoke(() =>
		{
			this.AllowMouseCapture = this.Settings.AllowMouseCapture;
		});
	}
}
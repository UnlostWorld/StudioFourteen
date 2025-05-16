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

namespace StudioFourteen.Rendering.Scene.Gizmos;

using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Serilog;
using WpfUtils.Extensions;

public partial class GizmoControlPanel : UserControl
{
	protected readonly ILogger Log;

	public GizmoControlPanel()
	{
		this.InitializeComponent();
		this.DataContext = this;
		this.Log = Logging.ForContext<GizmoControlPanel>();

		this.Services.Gizmos.GizmosChanged += this.OnGizmosChanged;
		this.Gizmos.Replace(this.Services.Gizmos.Gizmos);
	}

	public FastObservableCollection<GizmoBase> Gizmos { get; init; } = new();
	public ServiceManager Services => ServiceManager.Instance;

	private void OnGizmosChanged()
	{
		this.Dispatcher.Invoke(() =>
		{
			if (this.Services.Gizmos.GizmoControlPanelOpen)
				return;

			this.Gizmos.Replace(this.Services.Gizmos.Gizmos);
		});
	}
}
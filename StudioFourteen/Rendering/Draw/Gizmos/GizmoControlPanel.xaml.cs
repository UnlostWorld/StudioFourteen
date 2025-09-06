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

namespace StudioFourteen.Rendering.Draw.Gizmos;

using System;
using System.Collections.Generic;
using System.Windows.Controls;
using Serilog;
using WpfUtils.Extensions;

public partial class GizmoControlPanel : UserControl
{
	protected readonly ILogger Log;

	private readonly Dictionary<Type, List<GizmoBase>> gizmoLookup = new();

	public GizmoControlPanel()
	{
		this.InitializeComponent();
		this.DataContext = this;
		this.Log = Logging.ForContext<GizmoControlPanel>();

		List<GizmoBase> currentGizmos = new(this.Services.Gizmos.Gizmos);
		this.Services.Gizmos.GizmoAdded += this.AddGizmo;
		this.Services.Gizmos.GizmoRemoved += this.RemoveGizmo;

		foreach (GizmoBase gizmo in currentGizmos)
		{
			this.AddGizmo(gizmo);
		}
	}

	public FastObservableCollection<GizmoBase> Gizmos { get; init; } = new();
	public ServiceManager Services => ServiceManager.Instance;

	private void AddGizmo(GizmoBase gizmo)
	{
		if (!gizmo.ShowInControlPanel)
			return;

		Type gizmoType = gizmo.GetType();

		if (!this.gizmoLookup.ContainsKey(gizmoType))
		{
			this.gizmoLookup.Add(gizmoType, new());
			this.Dispatcher.Invoke(() => this.Gizmos.Add(gizmo));
		}
		else
		{
			this.gizmoLookup[gizmoType].Add(gizmo);
		}
	}

	private void RemoveGizmo(GizmoBase gizmo)
	{
		Type gizmoType = gizmo.GetType();
		if (!this.gizmoLookup.ContainsKey(gizmoType))
			return;

		this.gizmoLookup[gizmoType].Remove(gizmo);

		if (this.Gizmos.Contains(gizmo))
		{
			this.Dispatcher.BeginInvoke(() =>
			{
				this.Gizmos.Remove(gizmo);

				if (this.gizmoLookup[gizmoType].Count > 0)
				{
					this.Gizmos.Add(this.gizmoLookup[gizmoType][0]);
				}
			});
		}
	}
}
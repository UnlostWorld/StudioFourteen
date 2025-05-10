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

namespace StudioFourteen.Rendering.Gizmos;

using System;
using System.Collections.Generic;
using StudioFourteen.Services;
using PropertyChanged.SourceGenerator;

public partial class GizmoService : ServiceBase
{
	public readonly List<GizmoBase> Gizmos = new();
	private readonly GridGizmo grid = new();

	[Notify] private bool gizmoControlPanelOpen;

	public delegate void GizmosChangedDelegate();
	public event GizmosChangedDelegate? GizmosChanged;

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.grid.Enable();
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.grid.Disable();
		base.Detach();
	}

	public bool IsEnabled(GizmoBase gizmo)
	{
		return this.Gizmos.Contains(gizmo);
	}

	public void Enable(GizmoBase gizmo)
	{
		this.Gizmos.Add(gizmo);
		this.GizmosChanged?.Invoke();

		this.Services.Rendering.Forward.Add(gizmo);
	}

	public void Disable(GizmoBase gizmo)
	{
		this.Gizmos.Remove(gizmo);
		this.GizmosChanged?.Invoke();

		this.Services.Rendering.Forward.Remove(gizmo);
	}

	private void OnGameTick()
	{
		foreach(GizmoBase gizmo in this.Gizmos)
		{
			gizmo.OnGameTick();
		}
	}
}
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

using System.Collections.Generic;
using StudioFourteen.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;
using StudioFourteen.Scene;

public partial class GizmoService : ServiceBase
{
	public readonly List<GizmoBase> Gizmos = new();

	public readonly TransformGizmo Transform = new();
	private readonly GridGizmo grid = new();
	private readonly SelectionGizmo selection = new();

	[Notify] private bool gizmoControlPanelOpen;

	public delegate void GizmoChangedDelegate(GizmoBase gizmo);
	public event GizmoChangedDelegate? GizmoAdded;
	public event GizmoChangedDelegate? GizmoRemoved;

	public override void Attach()
	{
		this.Services.Selection.SelectionChanged += this.OnSelectionChanged;
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		this.grid.Enable(this.Services.Rendering.OverlayRenderer.Forward);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Selection.SelectionChanged -= this.OnSelectionChanged;
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);

		this.grid.Disable();
		this.Transform.Disable();
		this.selection.Disable();
		base.Detach();
	}

	public bool IsEnabled(GizmoBase gizmo)
	{
		return this.Gizmos.Contains(gizmo);
	}

	public void Enable(GizmoBase gizmo)
	{
		this.Gizmos.Add(gizmo);
		this.GizmoAdded?.Invoke(gizmo);
	}

	public void Disable(GizmoBase gizmo)
	{
		this.Gizmos.Remove(gizmo);
		this.GizmoRemoved?.Invoke(gizmo);
	}

	private void OnSelectionChanged(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? selectionSource)
	{
		if (oldSelection != null)
		{
			this.Transform.Disable();
			this.selection.Disable();
		}

		if (newSelection != null)
		{
			this.Transform.Enable(newSelection, this.Services.Rendering.OverlayRenderer.Forward);
			this.selection.Enable(newSelection, this.Services.Rendering.OverlayRenderer.Forward);
		}
	}

	private void OnGameTick()
	{
		foreach (GizmoBase gizmo in this.Gizmos)
		{
			gizmo.OnGameTick();
		}
	}
}

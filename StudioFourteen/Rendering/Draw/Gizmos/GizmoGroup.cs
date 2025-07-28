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
using StudioFourteen.Rendering.Passes;

public abstract class GizmoGroup : GizmoBase
{
	private GizmoBase? current;

	public List<GizmoBase> Gizmos { get; set; } = new();

	public GizmoBase? Current
	{
		get => this.current;
		set
		{
			this.current?.Disable();
			this.current = value;

			if (this.renderPass == null)
				return;

			this.current?.Enable(this.renderPass);
		}
	}

	public override bool IsVisible
	{
		get => base.IsVisible;
		set
		{
			foreach (GizmoBase gizmo in this.Gizmos)
			{
				gizmo.IsVisible = value;
			}

			base.IsVisible = value;
		}
	}

	public override void Enable(ForwardPass? pass = null)
	{
		if (this.current == null && this.Gizmos.Count > 0)
			this.current = this.Gizmos[0];

		this.current?.Enable(pass);

		foreach (GizmoBase gizmo in this.Gizmos)
		{
			gizmo.IsVisible = this.IsVisible;
		}

		base.Enable(pass);
	}

	public override void Disable()
	{
		foreach (SceneObjectGizmoBase gizmo in this.Gizmos)
		{
			gizmo.Disable();
		}

		base.Disable();
	}

	protected void AddGizmo<T>()
		where T : GizmoBase, new()
	{
		this.Gizmos.Add(new T());
	}
}
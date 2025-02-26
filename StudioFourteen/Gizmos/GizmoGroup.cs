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

namespace StudioFourteen.Gizmos;

using StudioFourteen.Posing;
using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows;
using Serilog;
using StudioFourteen;
using StudioFourteen.Gizmos.Handles;

public class GizmoGroup : IGizmo
{
	public readonly List<IGizmo> Children = new();
	public Transform Transform = Transform.Identity;
	public bool KeepScreenSize = false;

	protected readonly ILogger Log;

	private readonly Queue<IGizmo> newGizmos = new();
	private readonly Queue<IGizmo> deleteGizmos = new();

	public GizmoGroup()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public ServiceManager Services => ServiceManager.Instance;
	public GizmoGroup? Parent { get; set; }
	public bool IsVisible { get; set; } = true;
	public bool IsCursorOver { get; private set; }
	public bool IsEnabled { get; private set; }

	public virtual void Enable(GizmoRenderer renderer)
	{
		this.IsEnabled = true;

		foreach (IGizmo gizmo in this.Children)
		{
			gizmo.Parent = this;
			gizmo.Enable(renderer);
		}
	}

	public virtual void Disable(GizmoRenderer renderer)
	{
		this.IsEnabled = false;

		foreach (IGizmo izmo in this.Children)
		{
			izmo.Disable(renderer);
		}
	}

	public virtual void Update(Matrix4x4 view, Matrix4x4 projection, GizmoRenderer renderer)
	{
		while (this.deleteGizmos.Count > 0)
		{
			IGizmo gizmo = this.deleteGizmos.Dequeue();
			gizmo.Parent = null;
			gizmo.Disable(renderer);
			this.Children.Remove(gizmo);
		}

		while (this.newGizmos.Count > 0)
		{
			IGizmo gizmo = this.newGizmos.Dequeue();
			gizmo.Parent = this;
			gizmo.Enable(renderer);
			this.Children.Add(gizmo);
		}

		foreach (IGizmo gizmo in this.Children)
		{
			gizmo.Update(view, projection, renderer);
		}
	}

	public Transform GetTransform()
	{
		if (this.Parent != null)
			return this.Parent.GetTransform() * this.Transform;

		return this.Transform;
	}

	public bool GetIsVisible()
	{
		if (this.Parent != null)
			return this.Parent.GetIsVisible() && this.IsVisible;

		return this.IsVisible;
	}

	public bool GetKeepScreenSize()
	{
		if (this.Parent != null)
			return this.Parent.GetKeepScreenSize() || this.KeepScreenSize;

		return this.KeepScreenSize;
	}

	public virtual void HitTest(Point mousePos, ref HandleHitResult result)
	{
		foreach (IGizmo gizmo in this.Children)
		{
			if (!gizmo.IsVisible)
				continue;

			gizmo.HitTest(mousePos, ref result);
		}
	}

	protected T AddChild<T>()
		where T : IGizmo, new()
	{
		T gizmo = new T();
		this.newGizmos.Enqueue(gizmo);
		return gizmo;
	}

	protected void AddChild(IGizmo gizmo)
	{
		this.newGizmos.Enqueue(gizmo);
	}

	protected void RemoveChild(IGizmo gizmo)
	{
		this.deleteGizmos.Enqueue(gizmo);
	}
}

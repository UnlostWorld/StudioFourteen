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

	private readonly Queue<IGizmo> newPrimitives = new();
	private readonly Queue<IGizmo> deletePrimitives = new();

	public GizmoGroup()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public ServiceManager Services => ServiceManager.Instance;
	public GizmoGroup? Parent { get; set; }
	public bool IsVisible { get; set; } = true;
	public bool IsCursorOver { get; private set; }

	public virtual void Enable(Canvas canvas)
	{
		foreach (IGizmo primitive in this.Children)
		{
			primitive.Parent = this;
			primitive.Enable(canvas);
		}
	}

	public virtual void Disable(Canvas canvas)
	{
		foreach (IGizmo primitive in this.Children)
		{
			primitive.Disable(canvas);
		}
	}

	public virtual void Update(Matrix4x4 view, Matrix4x4 projection, Canvas canvas)
	{
		while (this.deletePrimitives.Count > 0)
		{
			IGizmo primitive = this.deletePrimitives.Dequeue();
			primitive.Parent = null;
			primitive.Disable(canvas);
			this.Children.Remove(primitive);
		}

		while (this.newPrimitives.Count > 0)
		{
			IGizmo primitive = this.newPrimitives.Dequeue();
			primitive.Parent = this;
			primitive.Enable(canvas);
			this.Children.Add(primitive);
		}

		foreach (IGizmo primitive in this.Children)
		{
			primitive.Update(view, projection, canvas);
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
		foreach (IGizmo primitive in this.Children)
		{
			if (!primitive.IsVisible)
				continue;

			primitive.HitTest(mousePos, ref result);
		}
	}

	protected T AddChild<T>()
		where T : IGizmo, new()
	{
		T primitive = new T();
		this.newPrimitives.Enqueue(primitive);
		return primitive;
	}

	protected void AddChild(IGizmo primitive)
	{
		this.newPrimitives.Enqueue(primitive);
	}

	protected void RemoveChild(IGizmo primitive)
	{
		this.deletePrimitives.Enqueue(primitive);
	}
}

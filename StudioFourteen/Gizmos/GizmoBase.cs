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

using Serilog;
using StudioFourteen;
using StudioFourteen.Gizmos.Handles;
using StudioFourteen.Posing;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using Vector = System.Windows.Vector;

public interface IGizmo
{
	public bool IsVisible { get; set; }

	GizmoGroup? Parent { get; set; }

	void Update(Matrix4x4 view, Matrix4x4 projection, Canvas canvas);
	void Enable(Canvas canvas);
	void Disable(Canvas canvas);

	Transform GetTransform();
	bool GetKeepScreenSize();
	bool GetIsVisible();

	void HitTest(Point mousePos, ref HandleHitResult result);
}

public abstract class GizmoBase : IGizmo
{
	public bool KeepScreenSize = false;
	public bool IgnoreTransformScale = true;

	protected readonly ILogger Log;

	private readonly List<FrameworkElement> elements = new();

	private Canvas? parent;
	private float screenWidth = 0;
	private float screenHeight = 0;
	private Matrix4x4 currentView;
	private Matrix4x4 currentProjection;
	private Matrix4x4 currentViewProjection;
	private Matrix4x4 currentTransform;
	private bool currentVisibility = true;

	public GizmoBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public Transform Transform { get; set; } = Transform.Identity;
	public GizmoGroup? Parent { get; set; }
	public bool IsVisible { get; set; } = true;

	protected ServiceManager Services => ServiceManager.Instance;

	protected Matrix4x4 View => this.currentView;
	protected Matrix4x4 Projection => this.currentProjection;
	protected Matrix4x4 ViewProjection => this.currentViewProjection;

	public virtual void Enable(Canvas canvas)
	{
		this.parent = canvas;
		this.screenWidth = (float)this.parent.ActualWidth;
		this.screenHeight = (float)this.parent.ActualHeight;
		this.currentVisibility = true;

		foreach (FrameworkElement el in this.elements)
		{
			canvas.Children.Add(el);
		}
	}

	public virtual void Update(Matrix4x4 view, Matrix4x4 projection, Canvas canvas)
	{
		if (this.parent == null)
			return;

		bool isVisible = this.GetIsVisible();
		if (this.currentVisibility != isVisible)
		{
			this.currentVisibility = isVisible;

			if (!isVisible)
			{
				this.Disable(this.parent);
			}
			else
			{
				this.Enable(this.parent);
			}
		}

		this.screenWidth = (float)this.parent.ActualWidth;
		this.screenHeight = (float)this.parent.ActualHeight;
		this.currentView = view;
		this.currentProjection = projection;
		this.currentViewProjection = view * projection;
		this.currentTransform = this.GetTransform().ToMatrix();

		if (this.IgnoreTransformScale)
		{
			if (Matrix4x4.Decompose(this.currentTransform, out Vector3 scale, out Quaternion rotation, out Vector3 translation))
			{
				this.currentTransform = Transform.FromTRS(translation, rotation, Vector3.One).ToMatrix();
			}
		}

		if (this.GetKeepScreenSize())
		{
			if (Matrix4x4.Invert(view, out Matrix4x4 invView))
			{
				Vector3 camPos = Vector3.Transform(Vector3.Zero, invView);
				Vector3 pos = Vector3.Transform(Vector3.Zero, this.currentTransform);

				float distance = (pos - camPos).Length();
				float scale = distance / 8f;
				this.currentTransform = Matrix4x4.CreateScale(scale) * this.currentTransform;
			}
		}

		this.Update();
	}

	public virtual void Update()
	{
	}

	public virtual void Disable(Canvas canvas)
	{
		foreach (FrameworkElement el in this.elements)
		{
			canvas.Children.Remove(el);
		}

		this.elements.Clear();
	}

	public Transform GetTransform()
	{
		if (this.Parent != null)
			return this.Parent.GetTransform() * this.Transform;

		return this.Transform;
	}

	public bool GetKeepScreenSize()
	{
		if (this.Parent != null)
			return this.Parent.GetKeepScreenSize() || this.KeepScreenSize;

		return this.KeepScreenSize;
	}

	public bool GetIsVisible()
	{
		if (this.Parent != null)
			return this.Parent.GetIsVisible() && this.IsVisible;

		return this.IsVisible;
	}

	public virtual void HitTest(Point mousePos, ref HandleHitResult result)
	{
	}

	public Vector3 LocalToScreen(Vector3 local)
	{
		Vector3 world = Vector3.Transform(local, this.currentTransform);
		Vector3 cameraPos = this.currentViewProjection.TransformViewProjection(world);
		Vector3 screenPos = new(cameraPos.X * this.screenWidth, cameraPos.Y * this.screenHeight, -cameraPos.Z);

		if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y) || float.IsNaN(screenPos.Z))
			return Vector3.Zero;

		return screenPos;
	}

	protected T AddChild<T>()
		where T : FrameworkElement, new()
	{
		T element = new T();
		this.elements.Add(element);
		return element;
	}

	protected void SetZIndex(FrameworkElement el, float depth)
	{
		Panel.SetZIndex(el, (int)(-depth * 100000));
	}
}
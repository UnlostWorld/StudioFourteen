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

namespace StudioFourteen.Overlays.Primitives;

using StudioFourteen.Posing;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

public interface IPrimitive
{
	public bool IsVisible { get; set; }

	PrimitiveGroup? Parent { get; set; }

	void Update(Matrix4x4 view, Matrix4x4 projection);
	void Enable(Canvas canvas);
	void Disable(Canvas canvas);

	Transform GetTransform();
	bool GetKeepScreenSize();
	bool GetIsVisible();
}

public abstract class PrimitiveBase : IPrimitive
{
	public bool KeepScreenSize = false;

	private readonly List<FrameworkElement> elements = new();

	private Canvas? parent;
	private float screenWidth = 0;
	private float screenHeight = 0;
	private Matrix4x4 currentViewProjection;
	private Matrix4x4 currentTransform;
	private bool currentVisibility = true;

	public Transform Transform { get; set; } = Transform.Identity;
	public PrimitiveGroup? Parent { get; set; }
	public bool IsVisible { get; set; } = true;

	public virtual void Enable(Canvas canvas)
	{
		this.parent = canvas;
		this.screenWidth = (float)this.parent.ActualWidth;
		this.screenHeight = (float)this.parent.ActualHeight;

		foreach (FrameworkElement el in this.elements)
		{
			canvas.Children.Add(el);
		}
	}

	public virtual void Update(Matrix4x4 view, Matrix4x4 projection)
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
		this.currentViewProjection = view * projection;
		this.currentTransform = this.GetTransform().ToMatrix();

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

	protected T AddChild<T>()
		where T : FrameworkElement, new()
	{
		T element = new T();
		this.elements.Add(element);
		return element;
	}

	protected void SetZIndex(FrameworkElement el, float depth)
	{
		Canvas.SetZIndex(el, (int)(-depth * 100000));
	}

	protected Vector3 LocalToScreen(Vector3 local)
	{
		Vector3 world = Vector3.Transform(local, this.currentTransform);
		Vector3 cameraPos = this.currentViewProjection.TransformViewProjection(world);
		Vector3 screenPos = new(cameraPos.X * this.screenWidth, cameraPos.Y * this.screenHeight, -cameraPos.Z);

		if (float.IsNaN(screenPos.X) || float.IsNaN(screenPos.Y) || float.IsNaN(screenPos.Z))
			return Vector3.Zero;

		return screenPos;
	}
}
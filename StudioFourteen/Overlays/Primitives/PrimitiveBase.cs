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
	void Update(Matrix4x4 viewProjection);
	void Enable(Canvas canvas);
	void Disable(Canvas canvas);
}

public abstract class PrimitiveBase : IPrimitive
{
	private readonly List<FrameworkElement> elements = new();

	private Canvas? parent;
	private float screenWidth = 0;
	private float screenHeight = 0;
	private Matrix4x4 currentViewProjection;

	public Transform Transform { get; set; } = Transform.Identity;

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

	public virtual void Update(Matrix4x4 viewProjection)
	{
		if (this.parent == null)
			return;

		this.screenWidth = (float)this.parent.ActualWidth;
		this.screenHeight = (float)this.parent.ActualHeight;
		this.currentViewProjection = viewProjection;

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

		this.parent = null;
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
		Canvas.SetZIndex(el, (int)(depth * 100000));
	}

	protected Vector3 LocalToScreen(Vector3 local)
	{
		Vector3 world = Vector3.Transform(local, this.Transform.ToMatrix());
		Vector3 cameraPos = this.currentViewProjection.TransformViewProjection(world);
		return new(cameraPos.X * this.screenWidth, cameraPos.Y * this.screenHeight, cameraPos.Z);
	}
}
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

using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;

public interface IPrimitive
{
	void Update();
	void Enable(Canvas canvas);
	void Disable(Canvas canvas);
}

public abstract class PrimitiveBase : IPrimitive
{
	private readonly List<FrameworkElement> elements = new();

	private Canvas? parent;
	private float screenWidth = 0;
	private float screenHeight = 0;

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

	public virtual void Update()
	{
		if (this.parent == null)
			return;

		this.screenWidth = (float)this.parent.ActualWidth;
		this.screenHeight = (float)this.parent.ActualHeight;
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

	protected bool Transform(Vector3 worldPos, out Vector3 screenPos)
	{
		bool isVisible = ServiceManager.Instance.Camera.WorldToCamera(worldPos, out Vector3 cameraPos);
		screenPos = new(cameraPos.X * this.screenWidth, cameraPos.Y * this.screenHeight, cameraPos.Z);

		return isVisible;
	}
}
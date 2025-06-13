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

namespace StudioFourteen.Rendering.Draw;

using System;
using System.Collections.Generic;
using System.Numerics;
using SharpDX.Direct3D11;

public class DrawGroup : DrawObject
{
	public readonly List<DrawObject> Children = new();

	public T Create<T>()
		where T : DrawObject, new()
	{
		T sceneObject = new T();
		sceneObject.Parent = this;
		this.Children.Add(sceneObject);
		return sceneObject;
	}

	public void Add(DrawObject sceneObject)
	{
		sceneObject.Parent = this;
		this.Children.Add(sceneObject);
	}

	public void Remove(DrawObject sceneObject)
	{
		if (sceneObject.Parent == this)
			sceneObject.Parent = null;

		this.Children.Remove(sceneObject);
	}

	public sealed override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		base.Draw(transform, device, deviceContext);

		if (!this.IsVisible)
			return;

		foreach (DrawObject child in this.Children)
		{
			try
			{
				child.Draw(this.WorldTransform, device, deviceContext);
			}
			catch (Exception ex)
			{
				child.IsVisible = false;
				this.Log.Error(ex, $"Error drawing scene object: {child}. This object will be disabled.");
			}
		}
	}

	public override void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		HitTestResult result)
	{
		if (!this.IsHitTestVisible || !this.IsVisible)
			return;

		Transform thisTransform = (this.LocalTransform * this.Transform) * transform;

		foreach (DrawObject child in this.Children)
		{
			if (!child.IsHitTestVisible || !child.IsVisible)
				continue;

			try
			{
				child.HitTest(screenPosition, thisTransform, viewProjection, result);
			}
			catch (Exception ex)
			{
				child.IsHitTestVisible = false;
				this.Log.Error(ex, $"Error hit testing scene object: {child}. This object will be disabled.");
			}
		}
	}

	public override void Dispose()
	{
		foreach (DrawObject child in this.Children)
		{
			child.Dispose();
		}
	}
}
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

namespace StudioFourteen.Rendering.Scene;

using System;
using System.Collections.Generic;
using System.Numerics;
using SharpDX.Direct3D11;

public class SceneGroup : SceneObject
{
	public readonly List<SceneObject> Children = new();

	public void Add(SceneObject sceneObject)
	{
		sceneObject.Parent = this;
		this.Children.Add(sceneObject);
	}

	public void Remove(SceneObject sceneObject)
	{
		if (sceneObject.Parent == this)
			sceneObject.Parent = null;

		this.Children.Remove(sceneObject);
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		this.OnDraw();

		if (!this.IsVisible)
			return;

		Transform thisTransform = this.Transform * transform;

		foreach (SceneObject child in this.Children)
		{
			try
			{
				child.Draw(thisTransform, device, deviceContext);
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

		Transform thisTransform = this.Transform * transform;

		foreach (SceneObject child in this.Children)
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
		foreach (SceneObject child in this.Children)
		{
			child.Dispose();
		}
	}

	protected virtual void OnDraw()
	{
	}
}
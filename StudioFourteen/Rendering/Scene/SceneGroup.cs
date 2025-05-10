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

using System.Collections.Generic;
using System.Numerics;
using SharpDX.Direct3D11;

public class SceneGroup : SceneObject
{
	public readonly List<SceneObject> Children = new();

	public virtual bool Visible { get; set; } = true;

	public void Add(SceneObject draw)
	{
		this.Children.Add(draw);
	}

	public void Remove(SceneObject draw)
	{
		this.Children.Remove(draw);
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.Visible)
			return;

		Transform thisTransform = transform * this.Transform;

		foreach(SceneObject child in this.Children)
		{
			child.Draw(thisTransform, device, deviceContext);
		}
	}

	public override void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		ref HitTestResult result)
	{
		Transform thisTransform = transform * this.Transform;

		foreach(SceneObject child in this.Children)
		{
			child.HitTest(screenPosition, thisTransform, viewProjection, ref result);
		}
	}

	public override void Dispose()
	{
		foreach(SceneObject child in this.Children)
		{
			child.Dispose();
		}
	}
}
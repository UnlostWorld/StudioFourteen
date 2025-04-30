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

namespace StudioFourteen.Rendering;

using System.Collections.Generic;
using System.Numerics;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Geometries;
using StudioFourteen.Rendering.Materials;

public class DrawGroup : DrawBase
{
	public readonly List<DrawBase> Children = new();

	public void Add(DrawBase draw)
	{
		this.Children.Add(draw);
	}

	public void Add(MaterialBase material, GeometryBase geometry)
	{
		this.Children.Add(new DrawObject(material, geometry));
	}

	public void Remove(DrawBase draw)
	{
		this.Children.Remove(draw);
	}

	public override void Draw(Transform transform, Device device, DeviceContext deviceContext)
	{
		Transform thisTransform = transform * this.Transform;

		foreach(DrawBase child in this.Children)
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

		foreach(DrawBase child in this.Children)
		{
			child.HitTest(screenPosition, thisTransform, viewProjection, ref result);
		}
	}

	public override void Dispose()
	{
		foreach(DrawBase child in this.Children)
		{
			child.Dispose();
		}
	}
}
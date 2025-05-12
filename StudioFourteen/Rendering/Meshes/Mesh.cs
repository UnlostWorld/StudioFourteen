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

using System;
using System.Collections.Generic;
using System.Numerics;
using SharpDX.Direct3D;

public class Mesh
{
	public PrimitiveTopology Topology;
	public List<Vertex> Vertices { get; set; } = new();
	public List<ushort>? Indices { get; set; }

	public void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		ref HitTestResult result)
	{
		if (this.Topology == PrimitiveTopology.LineStrip)
		{
			Vector4 from;
			Vector4 to;

			for (int i = 1; i < this.Vertices.Count; i++)
			{
				from = this.Vertices[i - 1].Position;
				from = Vector4.Transform(from, transform.ToMatrix());
				from = viewProjection.TransformViewProjection(from);

				to = this.Vertices[1].Position;
				to = Vector4.Transform(to, transform.ToMatrix());
				to = viewProjection.TransformViewProjection(to);

				// TODO: get closest point on line instead of this laziness.
				float fromDist = (screenPosition - from.AsVector2()).Length();
				if (fromDist < result.Distance)
				{
					result.MeshVertex = this.Vertices[i - 1];
					result.Distance = fromDist;
					result.Mesh = this;
				}

				float toDist = (screenPosition - to.AsVector2()).Length();
				if (toDist < result.Distance)
				{
					result.MeshVertex = this.Vertices[i];
					result.Distance = toDist;
					result.Mesh = this;
				}
			}
		}
		else
		{
			throw new NotSupportedException($"hit testing for {this.Topology} is not supported");
		}
	}
}
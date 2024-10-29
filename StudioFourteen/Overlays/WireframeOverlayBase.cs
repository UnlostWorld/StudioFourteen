namespace StudioFourteen.Overlays;

using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Shapes;

public abstract class WireframeOverlayBase
	: OverlayBase
{
	private readonly List<(Vector3 From, Vector3 To)> segments = new();
	private readonly List<(Vector3 From, Vector3 To)> screenSegments = new();
	private readonly List<Line> lines = new();

	/*public override unsafe void Transform(SceneCamera* pCamera)
	{
		for (int i = 0; i < this.segments.Count; i++)
		{
			CameraExtensions.WorldToCamera(pCamera, this.segments[i].From, out Vector3 fromScreenPos);
			CameraExtensions.WorldToCamera(pCamera, this.segments[i].To, out Vector3 toScreenPos);

			(Vector3 from, Vector3 to) = this.screenSegments[i];
			from = fromScreenPos;
			to = toScreenPos;
			this.screenSegments[i] = (from, to);
		}
	}*/

	public override void Update(Canvas canvas)
	{
		if (this.lines.Count != this.segments.Count)
		{
			foreach (Line oldLine in this.lines)
			{
				canvas.Children.Remove(oldLine);
			}

			this.lines.Clear();

			for (int i = 0; i < this.screenSegments.Count; i++)
			{
				Line line = new();
				this.lines.Add(line);
				canvas.Children.Add(line);
			}
		}

		for (int i = 0; i < this.screenSegments.Count; i++)
		{
			Line line = this.lines[i];

			Canvas.SetLeft(line, 0);
			Canvas.SetTop(line, 0);

			line.X1 = this.screenSegments[i].From.X;
			line.Y1 = this.screenSegments[i].From.Y;
			line.X2 = this.screenSegments[i].To.X;
			line.Y2 = this.screenSegments[i].To.Y;
		}
	}

	protected void AddLine(Vector3 from, Vector3 to)
	{
		this.segments.Add((from, to));
		this.screenSegments.Add((Vector3.Zero, Vector3.Zero));
	}
}

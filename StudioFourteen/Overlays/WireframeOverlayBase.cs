namespace StudioFourteen.Overlays;

using System.Collections.Generic;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

public abstract class WireframeOverlayBase
	: OverlayBase
{
	private readonly List<(Vector3 From, Vector3 To)> segments = new();
	private readonly List<Line> lines = new();

	public Vector3 Translation { get; set; } = Vector3.Zero;
	public Quaternion Rotation { get; set; } = Quaternion.Identity;
	public Vector3 Scale { get; set; } = Vector3.One;

	public override void Initialize(Canvas canvas)
	{
		base.Initialize(canvas);

		for (int i = 0; i < this.segments.Count; i++)
		{
			Line line = new();
			line.StrokeThickness = 1;
			line.Stroke = new SolidColorBrush(Colors.White);
			this.lines.Add(line);
			canvas.Children.Add(line);
		}
	}

	public override void Update(Canvas canvas)
	{
		for (int i = 0; i < this.segments.Count; i++)
		{
			Vector3 from = Vector3.Transform(this.segments[i].From * this.Scale, this.Rotation);
			Vector3 to = Vector3.Transform(this.segments[i].To * this.Scale, this.Rotation);
			this.Services.Camera.WorldToCamera(from + this.Translation, out Vector3 fromScreenPos);
			this.Services.Camera.WorldToCamera(to + this.Translation, out Vector3 toScreenPos);

			Line line = this.lines[i];

			Canvas.SetLeft(line, 0);
			Canvas.SetTop(line, 0);

			line.X1 = fromScreenPos.X * canvas.ActualWidth;
			line.Y1 = fromScreenPos.Y * canvas.ActualHeight;
			line.X2 = toScreenPos.X * canvas.ActualWidth;
			line.Y2 = toScreenPos.Y * canvas.ActualHeight;
		}
	}

	public override void Shutdown(Canvas canvas)
	{
		base.Shutdown(canvas);

		foreach(Line line in this.lines)
		{
			canvas.Children.Remove(line);
		}

		this.lines.Clear();
	}

	protected void AddLine(Vector3 from, Vector3 to)
	{
		this.segments.Add((from, to));
	}
}

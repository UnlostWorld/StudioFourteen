namespace StudioFourteen.Overlays;

public class BoxOverlay
	: WireframeOverlayBase
{
	public BoxOverlay()
	{
		this.AddLine(new(-0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, -0.5f));
		this.AddLine(new(0.5f, -0.5f, -0.5f), new(0.5f, 0.5f, -0.5f));
		this.AddLine(new(0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, -0.5f));
		this.AddLine(new(-0.5f, 0.5f, -0.5f), new(-0.5f, -0.5f, -0.5f));

		this.AddLine(new(-0.5f, -0.5f, 0.5f), new(0.5f, -0.5f, 0.5f));
		this.AddLine(new(0.5f, -0.5f, 0.5f), new(0.5f, 0.5f, 0.5f));
		this.AddLine(new(0.5f, 0.5f, 0.5f), new(-0.5f, 0.5f, 0.5f));
		this.AddLine(new(-0.5f, 0.5f, 0.5f), new(-0.5f, -0.5f, 0.5f));

		this.AddLine(new(-0.5f, -0.5f, -0.5f), new(-0.5f, -0.5f, 0.5f));
		this.AddLine(new(0.5f, -0.5f, -0.5f), new(0.5f, -0.5f, 0.5f));
		this.AddLine(new(0.5f, 0.5f, -0.5f), new(0.5f, 0.5f, 0.5f));
		this.AddLine(new(-0.5f, 0.5f, -0.5f), new(-0.5f, 0.5f, 0.5f));
	}
}
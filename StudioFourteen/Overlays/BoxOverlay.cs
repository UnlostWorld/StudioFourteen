namespace StudioFourteen.Overlays;

public class BoxOverlay
	: WireframeOverlayBase
{
	public BoxOverlay()
	{
		this.AddLine(new(0, 0, 0), new(1, 0, 0));
		this.AddLine(new(1, 0, 0), new(1, 1, 0));
		this.AddLine(new(1, 1, 0), new(0, 1, 0));
		this.AddLine(new(0, 1, 0), new(0, 0, 0));

		this.AddLine(new(0, 0, 1), new(1, 0, 1));
		this.AddLine(new(1, 0, 1), new(1, 1, 1));
		this.AddLine(new(1, 1, 1), new(0, 1, 1));
		this.AddLine(new(0, 1, 1), new(0, 0, 1));

		this.AddLine(new(0, 0, 0), new(0, 0, 1));
		this.AddLine(new(1, 0, 0), new(1, 0, 1));
		this.AddLine(new(1, 1, 0), new(1, 1, 1));
		this.AddLine(new(0, 1, 0), new(0, 1, 1));
	}
}
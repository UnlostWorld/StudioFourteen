namespace StudioFourteen.Overlays;

public class BoxOverlay
	: WireframeOverlayBase
{
	public BoxOverlay(string group, string name)
		: base(group, name)
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
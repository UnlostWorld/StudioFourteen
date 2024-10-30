namespace StudioFourteen.Overlays;

public class CameraWireframeOverlay : WireframeOverlayBase
{
	public CameraWireframeOverlay(string group, string name)
		: base(group, name)
	{
		// Body
		this.AddLine(new(-1.00f, -0.25f, +0.25f), new(-1.00f, -0.25f, -0.25f));
		this.AddLine(new(-1.00f, -0.25f, -0.25f), new(-1.00f, +0.25f, -0.25f));
		this.AddLine(new(-1.00f, +0.25f, -0.25f), new(-1.00f, +0.25f, +0.25f));
		this.AddLine(new(-1.00f, +0.25f, +0.25f), new(-1.00f, -0.25f, +0.25f));

		this.AddLine(new(-0.00f, -0.25f, +0.25f), new(+0.00f, -0.25f, -0.25f));
		this.AddLine(new(+0.00f, -0.25f, -0.25f), new(+0.00f, +0.25f, -0.25f));
		this.AddLine(new(+0.00f, +0.25f, -0.25f), new(-0.00f, +0.25f, +0.25f));
		this.AddLine(new(-0.00f, +0.25f, +0.25f), new(-0.00f, -0.25f, +0.25f));

		this.AddLine(new(-1.00f, -0.25f, +0.25f), new(-0.00f, -0.25f, +0.25f));
		this.AddLine(new(-1.00f, -0.25f, -0.25f), new(+0.00f, -0.25f, -0.25f));
		this.AddLine(new(-1.00f, +0.25f, -0.25f), new(+0.00f, +0.25f, -0.25f));
		this.AddLine(new(-1.00f, +0.25f, +0.25f), new(-0.00f, +0.25f, +0.25f));

		// Lens
		this.AddLine(new(+0.00f, +0.00f, +0.00f), new(+0.50f, -0.25f, +0.25f));
		this.AddLine(new(+0.00f, +0.00f, +0.00f), new(+0.50f, +0.25f, +0.25f));
		this.AddLine(new(+0.00f, +0.00f, +0.00f), new(+0.50f, -0.25f, -0.25f));
		this.AddLine(new(+0.00f, +0.00f, +0.00f), new(+0.50f, +0.25f, -0.25f));

		this.AddLine(new(+0.50f, -0.25f, +0.25f), new(+0.50f, -0.25f, -0.25f));
		this.AddLine(new(+0.50f, -0.25f, -0.25f), new(+0.50f, +0.25f, -0.25f));
		this.AddLine(new(+0.50f, +0.25f, -0.25f), new(+0.50f, +0.25f, +0.25f));
		this.AddLine(new(+0.50f, +0.25f, +0.25f), new(+0.50f, -0.25f, +0.25f));

		// film canisters curve
		this.AddLine(
			new(-0.11f, 0.25f, -0.00f),
			new(-0.14f, 0.32f, -0.00f),
			new(-0.10f, 0.36f, -0.00f),
			new(-0.07f, 0.40f, -0.00f),
			new(-0.05f, 0.45f, -0.00f),
			new(-0.05f, 0.51f, -0.00f),
			new(-0.06f, 0.56f, -0.00f),
			new(-0.08f, 0.61f, -0.00f),
			new(-0.12f, 0.65f, -0.00f),
			new(-0.16f, 0.68f, -0.00f),
			new(-0.21f, 0.70f, -0.00f),
			new(-0.27f, 0.71f, -0.00f),
			new(-0.32f, 0.70f, -0.00f),
			new(-0.37f, 0.68f, -0.00f),
			new(-0.41f, 0.65f, -0.00f),
			new(-0.59f, 0.65f, -0.00f),
			new(-0.63f, 0.68f, -0.00f),
			new(-0.68f, 0.70f, -0.00f),
			new(-0.73f, 0.71f, -0.00f),
			new(-0.79f, 0.70f, -0.00f),
			new(-0.84f, 0.68f, -0.00f),
			new(-0.88f, 0.65f, -0.00f),
			new(-0.92f, 0.61f, -0.00f),
			new(-0.94f, 0.56f, -0.00f),
			new(-0.95f, 0.51f, -0.00f),
			new(-0.95f, 0.45f, -0.00f),
			new(-0.93f, 0.40f, -0.00f),
			new(-0.90f, 0.36f, -0.00f),
			new(-0.86f, 0.32f, -0.00f),
			new(-0.89f, 0.25f, -0.00f));

		this.Scale = new(0.5f, 0.5f, 0.5f);
	}
}

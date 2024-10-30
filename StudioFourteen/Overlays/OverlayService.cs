namespace StudioFourteen.Overlays;

using StudioFourteen.Services;
using System.Collections.Generic;

public interface IOverlayOwner
{
	string OverlayName { get; }
}

public class OverlayService
	: ServiceBase
{
	private readonly List<OverlayBase> overlays = new();

	public delegate void OverlayEvent(OverlayBase overlay);

	public event OverlayEvent? OverlayAdded;
	public event OverlayEvent? OverlayRemoved;

	public void AddOverlay(OverlayBase overlay)
	{
		this.OverlayAdded?.Invoke(overlay);
	}

	public void RemoveOverlay(OverlayBase overlay)
	{
		this.OverlayRemoved?.Invoke(overlay);
	}

	public List<OverlayBase> GetOverlays()
	{
		return this.overlays;
	}
}

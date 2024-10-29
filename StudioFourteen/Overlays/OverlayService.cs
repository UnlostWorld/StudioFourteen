namespace StudioFourteen.Overlays;

using StudioFourteen.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IOverlayOwner
{
	string OverlayName { get; }
}

public class OverlayService
	: ServiceBase
{
	public List<OverlayBase> Overlays { get; init; } = new();

	public override Task Initialize()
	{
		////this.Overlays.Add(new PointOverlay());

		return base.Initialize();
	}
}

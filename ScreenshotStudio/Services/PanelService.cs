// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using ScreenshotStudio.Studio;
using ScreenshotStudio.Windows;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

public class PanelService : ServiceBase
{
	public ObservableCollection<Panel> OpenPanels { get; init; } = new();

	public override async Task Stop()
	{
		await base.Stop();

		foreach (Panel? panel in this.OpenPanels.Reverse())
		{
			if (panel == null)
				continue;

			await panel.CloseAsync();
		}
	}
}

namespace StudioFourteen.Overlays;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Serilog;
using StudioFourteen.Utilities;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

public class OverlayRenderer : Canvas
{
	protected readonly ILogger Log = Logging.ForContext<OverlayRenderer>();

	public OverlayRenderer()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Studio.Opening += this.OnStudioOpening;
	}

	protected ServiceManager Services => ServiceManager.Instance;

	private void OnStudioOpening()
	{
		this.RenderTask().Run();
	}

	private async Task RenderTask()
	{
		try
		{
			while (this.Services.Overlays.IsAlive && this.Services.Overlays.IsAttached)
			{
				await Task.Delay(1000 / 60);
				await this.MainThread();

				foreach (OverlayBase overlay in this.Services.Overlays.Overlays)
				{
					try
					{
						overlay.Update(this);
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Error in overlay {overlay}");
					}
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in overlay renderer");
		}
	}
}

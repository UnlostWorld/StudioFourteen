namespace StudioFourteen.Overlays;

using DependencyPropertyGenerator;
using Serilog;
using StudioFourteen.Plugin;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

[DependencyProperty<bool>("ShowOverlays")]
public partial class OverlayRenderer : Canvas
{
	protected readonly ILogger Log = Logging.ForContext<OverlayRenderer>();

	// How many frames of focus do we wait before showing overlays again
	private const int RequiredFocusCount = 10;
	private int focusCount = 0;

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

				if (DalamudServices.DalamudHasFocus || AtkManager.HasActiveWindow())
				{
					this.focusCount = 0;
					this.ShowOverlays = false;
					continue;
				}
				else if (this.focusCount <= RequiredFocusCount)
				{
					this.focusCount++;
					this.ShowOverlays = false;
					continue;
				}
				else
				{
					this.ShowOverlays = true;
				}

				for (int i = this.Services.Overlays.Overlays.Count - 1; i > 0; i--)
				{
					OverlayBase overlay = this.Services.Overlays.Overlays[i];
					try
					{
						if (!overlay.IsInitialized)
						{
							overlay.Initialize(this);
						}

						overlay.Update(this);

						if (overlay.IsShuttingDown)
						{
							overlay.Shutdown(this);
						}
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Error in overlay {overlay}");
					}
				}
			}

			this.Visibility = Visibility.Collapsed;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in overlay renderer");
		}
	}
}

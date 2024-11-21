// .                    @@             _____ _______ _    _ _____ _____ ____
//          @       @@@@@             / ____|__   __| |  | |  __ \_   _/ __ \
//         @@@  @@@@                 | (___    | |  | |  | | |  | || || |  | |
//         @@@@@@@@@  @    @          \___ \   | |  | |  | | |  | || || |  | |
//        @@@@       @@@@@@@          ____) |  | |  | |__| | |__| || || |__| |
//    @@@@@             @@@          |_____/   |_|   \____/|_____/_____\____/
//     @@@      @@@      @@        ___     _    _   _  __   _____  ___  ___  _  _
//      @@    @@@@@@@    @@       |  _|  / _ \ | | | || _ \|_   _|| __|| __|| \| |
//      @@    @@@@@@@    @   @    | __| | (_) || |_| ||   /  | |  | _| | _| | .` |
//    @@@@      @@@      @@@@     |_|    \___/  \___/ |_|_\  |_|  |___||___||_|\_|
//     @@@@             @@@        https://github.com/UnlostWorld/StudioFourteen
//       @@@@@      @@@@@
//        @@@@@@@@@@@@@@                This software is licensed under the
//            @@@@  @                  GNU AFFERO GENERAL PUBLIC LICENSE v3

namespace StudioFourteen.Overlays;

using DependencyPropertyGenerator;
using Serilog;
using StudioFourteen.Plugin;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WpfUtils;
using WpfUtils.Extensions;

public partial class OverlayRenderer : Canvas
{
	protected readonly ILogger Log = Logging.ForContext<OverlayRenderer>();

	private readonly List<OverlayBase> overlays = new();

	public OverlayRenderer()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
			return;

		this.Services.Studio.Opening += this.OnStudioOpening;
		this.Services.Overlays.OverlayAdded += this.OnOverlayAdded;
		this.Services.Overlays.OverlayRemoved += this.OnOverlayRemoved;

		this.overlays.AddRange(this.Services.Overlays.GetOverlays());
	}

	protected ServiceManager Services => ServiceManager.Instance;

	private void OnStudioOpening()
	{
		this.RenderTask().Run();
	}

	private void OnOverlayAdded(OverlayBase overlay)
	{
		this.overlays.Add(overlay);

		this.Dispatcher.Invoke(() =>
		{
			overlay.Initialize(this);
		});
	}

	private void OnOverlayRemoved(OverlayBase overlay)
	{
		this.overlays.Remove(overlay);

		this.Dispatcher.Invoke(() =>
		{
			overlay.Shutdown(this);
		});
	}

	private async Task RenderTask()
	{
		try
		{
			while (!ServiceManager.ShutdownRequested
				&& this.Services.Overlays.IsAlive
				&& this.Services.Overlays.IsAttached)
			{
				await Task.Delay(1000 / 60);
				await this.MainThread();

				for (int i = this.overlays.Count - 1; i >= 0; i--)
				{
					OverlayBase overlay = this.overlays[i];

					try
					{
						if (overlay.IsHidden && overlay.IsInitialized)
						{
							overlay.Shutdown(this);
						}
						else if (!overlay.IsHidden && !overlay.IsInitialized)
						{
							overlay.Initialize(this);
						}
						else
						{
							overlay.Update(this);
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

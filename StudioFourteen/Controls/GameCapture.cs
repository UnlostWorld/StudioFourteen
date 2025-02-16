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

namespace StudioFourteen.Controls;

using StudioFourteen;
using StudioFourteen.Services;
using Serilog;
using SixLabors.ImageSharp;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using WpfUtils;

public class GameCapture : System.Windows.Controls.Image, ICaptureListener
{
	protected readonly ILogger Log;

	private WriteableBitmap? bitmap;
	private bool hasCapture = false;

	public GameCapture()
	{
		this.Log = Logging.ForContext<GameCapture>();

		if (DesignerProperties.GetIsInDesignMode(this))
		{
			this.Source = new BitmapImage(new Uri("http://dev.mos.cms.futurecdn.net/HhsJyWHPnQsuojX9GcwWKe.jpg"));
			return;
		}

		this.IsVisibleChanged += this.OnIsVisibleChanged;
		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
		this.Dispatcher.ShutdownStarted += this.OnDispatcherShutdownStarted;
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public void OnCapture()
	{
		this.hasCapture = true;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (this.IsVisible)
			ServiceManager.Instance.GameCapture.AddListener(this);

		Task.Run(this.UpdateLoop);
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		ServiceManager.Instance.GameCapture.RemoveListener(this);
	}

	private void OnDispatcherShutdownStarted(object? sender, EventArgs e)
	{
		ServiceManager.Instance.GameCapture.RemoveListener(this);
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible && this.IsLoaded)
		{
			ServiceManager.Instance.GameCapture.AddListener(this);
		}
		else
		{
			ServiceManager.Instance.GameCapture.RemoveListener(this);
		}
	}

	private async Task UpdateLoop()
	{
		try
		{
			await this.Dispatcher.MainThread();

			while (this.IsLoaded)
			{
				await this.Dispatcher.MainThread();

				if (this.hasCapture && this.IsVisible)
				{
					if (this.Services.Photos.ShowDepth)
					{
						this.Services.GameCapture.DrawDepthBufferToBitmap(ref this.bitmap);
					}
					else
					{
						this.Services.GameCapture.DrawBackBufferToBitmap(ref this.bitmap);
					}

					this.Source = this.bitmap;
					this.hasCapture = false;
				}

				await Task.Delay(30);
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in Game Capture control update loop");
		}
	}
}

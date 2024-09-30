namespace ScreenshotStudio.Controls;

using ScreenshotStudio;
using ScreenshotStudio.Services;
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

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
		this.Dispatcher.ShutdownStarted += this.OnDispatcherShutdownStarted;
	}

	protected ILogger Log { get; private set; }

	public void OnCapture()
	{
		this.hasCapture = true;
	}

	public Image? ToImage()
	{
		return ServiceManager.Instance.GameCapture.ToImage();
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
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

	private async Task UpdateLoop()
	{
		try
		{
			await this.Dispatcher.MainThread();

			while (this.IsLoaded)
			{
				await this.Dispatcher.MainThread();

				if (this.hasCapture)
				{
					ServiceManager.Instance.GameCapture.DrawBitmap(ref this.bitmap);
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

namespace ScreenshotStudio.Studio;
using ScreenshotStudio.Services;
using ScreenshotStudio.Utilities;
using ScreenshotStudio.Windows;
using System;
using System.Windows;

public partial class BackgroundWindow : PersistentPanel
{
	public BackgroundWindow()
	{
		this.ContentArea.DataContext = this;
	}

	[AutoNotify]
	public bool IsFullyLoaded
	{
		get
		{
			return this.Services.CurrentState == ServiceManagerBase.States.Started;
		}
	}

	protected override void OnOpened()
	{
		base.OnOpened();
		this.UpdatePosition();
		XivWindow.Activate();
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);
		this.UpdatePosition();
		XivWindow.Activate();
	}

	private void UpdatePosition()
	{
		this.Width = XivWindow.Size.Width;
		this.Height = XivWindow.Size.Height - XivWindow.TitleBarHeight;

		XivWindow.SetPosition(this, new(0, 0));
	}

	private void OnStudioClicked(object sender, RoutedEventArgs e)
	{
		if (this.Services.Studio.IsOpen)
		{
			this.Services.Studio.CloseStudio();
		}
		else
		{
			this.Services.Studio.OpenStudio();
		}
	}
}

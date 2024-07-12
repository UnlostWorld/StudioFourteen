namespace ScreenshotStudio;

using ScreenshotStudio.Services;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;

public class View : UserControl, IAutoNotify
{
	protected readonly ILogger Log;

	public View()
	{
		if (DesignerProperties.GetIsInDesignMode(this))
		{
			this.Log = null!;
		}
		else
		{
			this.Log = Logging.ForContext(this.GetType());
		}

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);

		this.Loaded += this.OnLoaded;
		this.Unloaded += this.OnUnloaded;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	public bool ShouldTickAutoProperties()
	{
		return this.IsVisible;
	}

	private void OnLoaded(object sender, RoutedEventArgs e)
	{
		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			AutoPropertyNotifyService.Register(this);
		}
	}

	private void OnUnloaded(object sender, RoutedEventArgs e)
	{
		if (!DesignerProperties.GetIsInDesignMode(this))
		{
			AutoPropertyNotifyService.Remove(this);
		}
	}
}
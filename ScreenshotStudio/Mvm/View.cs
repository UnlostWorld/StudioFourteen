namespace ScreenshotStudio.Mvm;

using Dalamud.Plugin.Services;
using ScreenshotStudio.Mvm;
using ScreenshotStudio.Plugin;
using Serilog;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

public class View : UserControl, IAutoNotify
{
	protected readonly ILogger Log;

	public View()
	{
		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);

		if (this.Content is FrameworkElement el)
		{
			el.DataContext = this;
		}

		if (DesignerProperties.GetIsInDesignMode(this))
		{
			this.Log = null!;
			return;
		}

		this.Log = Logging.ForContext(this.GetType());

		this.Loaded += (s, e) =>
		{
			try
			{
				this.OnLoaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error loading View");
			}
		};

		this.Unloaded += (s, e) =>
		{
			try
			{
				this.OnUnloaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error unloading View");
			}
		};

		this.Dispatcher.ShutdownStarted += (s, e) =>
		{
			try
			{
				this.OnUnloaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error unloading View");
			}
		};
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public ServiceManager Services => ServiceManager.Instance;

	public void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new(propertyName));
	}

	public bool ShouldTickAutoProperties()
	{
		return this.IsVisible;
	}

	protected override void OnContentChanged(object oldContent, object newContent)
	{
		base.OnContentChanged(oldContent, newContent);

		if (this.Content is FrameworkElement el)
		{
			el.DataContext = this;
		}
	}

	protected virtual void OnLoaded()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdate;

		AutoPropertyNotifyService.Register(this);
	}

	protected virtual void OnUnloaded()
	{
		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdate;

		AutoPropertyNotifyService.Remove(this);
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}
}
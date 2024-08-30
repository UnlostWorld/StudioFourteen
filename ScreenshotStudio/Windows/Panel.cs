namespace ScreenshotStudio.Windows;

using Dalamud.Plugin.Services;
using DependencyPropertyGenerator;
using FontAwesome.Sharp;
using ScreenshotStudio.Plugin;
using ScreenshotStudio.Serialization;
using ScreenshotStudio.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

[DependencyProperty<bool>("IsShown", DefaultValue = false)]
[DependencyProperty<IconChar>("TitleIcon")]
[DependencyProperty<string>("Title")]
[DependencyProperty<string>("Subtitle")]
[DependencyProperty<SizeToContent>("SizeToContent", DefaultValue =SizeToContent.Manual)]
[DependencyProperty<ResizeMode>("ResizeMode", DefaultValue =ResizeMode.CanResizeWithGrip)]
public partial class Panel : ContentControl, IAutoNotify
{
	protected readonly ILogger Log;

	private readonly string panelId;
	private readonly Dictionary<string, object?> persistenceCache = new();
	private Exception? frameworkException;
	private IHost? host;

	private bool isVisible;

	public Panel()
	{
		this.panelId = this.GetType().Name;
		this.Log = Logging.ForContext(this.GetType());

		// Load a new copy of the resources. Each panel needs its own instance for threading reasons.
		this.Resources = ScreenshotStudio.Resources.Load();

		this.GetType().GetMethod("InitializeComponent")?.Invoke(this, null);
		this.DataContext = this;

		this.IsVisibleChanged += (s, e) => this.isVisible = this.IsVisible;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public interface IHost
	{
		void Close();
	}

	public ServiceManager Services => ServiceManager.Instance;

	public virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
	}

	public virtual bool ShouldTickAutoProperties()
	{
		return this.isVisible;
	}

	public void SetHost(IHost host)
	{
		this.host = host;
	}

	public void Close()
	{
		this.host?.Close();
	}

	public T? GetPersistence<T>([CallerMemberName] string id = "")
	{
		try
		{
			lock (this.persistenceCache)
			{
				if (this.persistenceCache.ContainsKey(id))
				{
					return (T?)this.persistenceCache[id];
				}
			}

			string persistenceId = this.panelId + "_" + id;

			if (!this.Services.Settings.Current.PanelPersistence.TryGetValue(persistenceId, out string? json) || json == null)
				return default;

			if (!json.StartsWith('"') || !json.EndsWith('"'))
				json = '"' + json + '"';

			T? value = Serializer.Deserialize<T>(json);
			this.persistenceCache.Add(id, value);
			return value;
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
			return default;
		}
	}

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.SetPersistence(id, value);
	}

	public void SetPersistence(string id, object? value)
	{
		try
		{
			lock (this.persistenceCache)
			{
				if (!this.persistenceCache.ContainsKey(id))
					this.persistenceCache.Add(id, value);

				this.persistenceCache[id] = value;
			}

			this.Dispatcher.Invoke(() =>
			{
				string persistenceId = this.panelId + "_" + id;

				if (value != null)
				{
					if (!this.Services.Settings.Current.PanelPersistence.ContainsKey(persistenceId))
						this.Services.Settings.Current.PanelPersistence.Add(persistenceId, string.Empty);

					this.Services.Settings.Current.PanelPersistence[persistenceId] = Serializer.Serialize(value);
				}
				else
				{
					if (this.Services.Settings.Current.PanelPersistence.ContainsKey(persistenceId))
					{
						this.Services.Settings.Current.PanelPersistence.Remove(persistenceId);
					}
				}
			});

			this.Services.Settings.Save();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in panel persistence");
		}
	}

	public void SetIsOpen(IHost sender, bool isOpen)
	{
		if (this.host != sender)
			throw new InvalidOperationException();

		if (isOpen)
		{
			this.OnOpened();
		}
		else
		{
			this.OnClosed();
		}
	}

	protected virtual void OnOpened()
	{
		this.Services.Panels.OnPanelOpened(this);

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update += this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Register(this);
		this.IsShown = true;
	}

	protected virtual void OnClosed()
	{
		this.Services.Panels.OnPanelClosed(this);

		if (DalamudServices.Framework != null)
			DalamudServices.Framework.Update -= this.OnFrameworkUpdateSafe;

		AutoPropertyNotifyService.Remove(this);
		this.IsShown = false;
	}

	protected virtual void OnFrameworkUpdate(IFramework framework)
	{
	}

	private void OnFrameworkUpdateSafe(IFramework framework)
	{
		if (this.frameworkException != null)
			return;

		try
		{
			this.OnFrameworkUpdate(framework);
		}
		catch (Exception ex)
		{
			this.frameworkException = ex;
			this.Log.Error(ex, "Error in framework update");
		}
	}
}
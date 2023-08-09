namespace ScreenshotStudio.Services;

using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public abstract class ServiceBase : INotifyPropertyChanged
{
	public bool IsAlive
	{
		get;
		private set;
	}

	protected ILogger Log => Serilog.Log.ForContext(this.GetType());

	protected static ServiceManager Services => ServiceManager.Instance;

	public event PropertyChangedEventHandler? PropertyChanged;

	public virtual Task Initialize()
	{
		this.IsAlive = true;
		return Task.CompletedTask;
	}

	public virtual Task Shutdown()
	{
		this.IsAlive = false;
		return Task.CompletedTask;
	}

	public virtual Task Start()
	{
		return Task.CompletedTask;
	}

	protected virtual void RaisePropertyChanged(string property)
	{
		this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
	}
}

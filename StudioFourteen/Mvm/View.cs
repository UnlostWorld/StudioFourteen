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

namespace StudioFourteen.Mvm;

using Dalamud.Plugin.Services;
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
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
		this.Resources = StudioFourteen.Resources.Load();

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

	public virtual bool ShouldTickAutoProperties()
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
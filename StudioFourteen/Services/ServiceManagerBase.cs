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

namespace StudioFourteen.Services;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Tasks;
using Serilog;
using TerraFX.Interop.Windows;

public class ServiceManagerBase
{
	private static ServiceManagerBase? instance;
	private readonly List<ServiceBase> services = new();
	private States state = States.None;
	private bool isTicking = false;

	public ServiceManagerBase()
	{
		this.Log = Logging.ForContext(this.GetType());
		instance = this;

		PropertyInfo[] properties = this.GetType().GetProperties();
		foreach (PropertyInfo property in properties)
		{
			if (property.GetValue(this) is ServiceBase service)
			{
				this.services.Add(service);
			}
		}
	}

	public enum States
	{
		None,
		Initializing,
		Initialized,
		Starting,
		Started,
		Stopping,
		Stopped,
		ShuttingDown,
		ShutDown,
	}

	public static ServiceManagerBase Instance
	{
		get
		{
			if (instance == null)
				throw new Exception("No Service Manager");

			return instance;
		}
	}

	public static bool ShutdownRequested { get; private set; } = false;
	public States CurrentState => this.state;
	public ILogger Log { get; private set; }

	/// <summary>
	/// Initialize and Start all services.
	/// </summary>
	public async Task Start()
	{
		this.state = States.Initializing;

		ShutdownRequested = false;

		Logging.Shared.Information($"Studio Fourteen is initializing {this.services.Count} services");

		try
		{
			this.OnStart();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in service manager OnStart");
		}

		foreach (ServiceBase service in this.services)
		{
			try
			{
				await service.Initialize();

				if (ShutdownRequested)
					break;
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error initializing service: {service}");
			}
		}

		this.state = States.Initialized;

		this.Log.Information($"Studio Fourteen is starting {this.services.Count} services");
		this.state = States.Starting;

		foreach (ServiceBase service in this.services)
		{
			try
			{
				await service.Start();

				if (ShutdownRequested)
					break;
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error starting service: {service}");
			}
		}

		this.state = States.Started;

		_ = Task.Run(async () => await this.Tick());

		this.Log.Information("Studio Fourteen has started");
	}

	/// <summary>
	/// Stop and shutdown all services.
	/// </summary>
	public async Task Stop()
	{
		this.Log.Information("Studio Fourteen shut down requested");

		ShutdownRequested = true;

		this.OnStop();

		// Wait until Start() is done before stopping.
		while (this.state < States.Started)
		{
			this.Log.Information("Awaiting start completion");
			await Task.Delay(1000);
		}

		this.state = States.Stopping;

		// wait for any in progress ticks
		while (this.isTicking)
			await Task.Delay(100);

		this.Detach();

		foreach (ServiceBase service in this.services)
		{
			try
			{
				await service.Stop();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error stopping service: {service}");
			}
		}

		this.state = States.Stopped;
		this.state = States.ShuttingDown;

		foreach (ServiceBase service in this.services)
		{
			try
			{
				await service.Shutdown();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error shutting down service: {service}");
			}
		}

		this.services.Clear();

		this.Log.Information("Studio Fourteen has shut down");
		instance = null;

		this.state = States.ShutDown;
	}

	public void Attach()
	{
		foreach (ServiceBase service in this.services)
		{
			try
			{
				service.Attach();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error attaching service: {service}");
			}
		}
	}

	public void Detach()
	{
		foreach (ServiceBase service in this.services)
		{
			try
			{
				service.Detach();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error detaching service: {service}");
			}
		}
	}

	public void Dispose()
	{
		foreach (ServiceBase service in this.services)
		{
			try
			{
				service.Dispose();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, $"Error disposing service: {service}");
			}
		}
	}

	public ServiceBase GetService(Type type)
	{
		foreach (ServiceBase service in this.services)
		{
			if (service.GetType() == type)
			{
				return service;
			}
		}

		throw new Exception($"Service: {type} not found");
	}

	protected virtual void OnStart()
	{
	}

	protected virtual void OnStop()
	{
	}

	private async Task Tick()
	{
		while (this.state == States.Started)
		{
			await Task.Delay(10);

			this.isTicking = true;
			foreach (ServiceBase service in this.services)
			{
				if (ShutdownRequested)
					break;

				try
				{
					if (service.IsAlive)
					{
						await service.Tick();
					}
				}
				catch (Exception ex)
				{
					Logging.Shared.Error(ex, "Error ticking services");
				}
			}

			this.isTicking = false;
		}
	}
}

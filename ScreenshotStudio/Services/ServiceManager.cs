// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Library;

public class ServiceManager
{
	private static ServiceManager? instance;
	private readonly List<ServiceBase> services = new();
	private bool isRunning = false;

	private ServiceManager()
	{
		this.services.Add(this.Panels);
		this.services.Add(this.Data);
		this.services.Add(this.AutoNotify);
		this.services.Add(this.Library);
		this.services.Add(this.ActorLifecycle);
		this.services.Add(this.Studio);
	}

	public static ServiceManager Instance
	{
		get
		{
			if (instance == null)
				instance = new();

			return instance;
		}
	}

	// Service properties for bindings
	public AutoPropertyNotifyService AutoNotify { get; init; } = new();
	public PanelService Panels { get; init; } = new();
	public GameDataService Data { get; init; } = new();
	public LibraryService Library { get; init; } = new();
	public ActorLifecycleService ActorLifecycle { get; init; } = new();
	public StudioService Studio { get; init; } = new();

	/// <summary>
	/// Initialize and Start all services.
	/// </summary>
	public async Task Start()
	{
		this.isRunning = true;

		foreach (ServiceBase service in this.services)
		{
			await service.Initialize();
		}

		foreach (ServiceBase service in this.services)
		{
			await service.Start();
		}

		_ = Task.Run(async () => await this.Tick());
	}

	/// <summary>
	/// Stop and shutdown all services.
	/// </summary>
	public async Task Stop()
	{
		this.isRunning = false;

		foreach (ServiceBase service in this.services)
		{
			await service.Stop();
		}

		foreach (ServiceBase service in this.services)
		{
			await service.Shutdown();
		}
	}

	private async Task Tick()
	{
		while (this.isRunning)
		{
			foreach (ServiceBase service in this.services)
			{
				await Task.Delay(10);

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
		}
	}
}

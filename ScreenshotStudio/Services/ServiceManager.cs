// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Library;

public class ServiceManager
{
	private static ServiceManager? instance;
	private readonly List<ServiceBase> services = new();

	private ServiceManager()
	{
		this.services.Add(this.Targets);
		this.services.Add(this.Panels);
		this.services.Add(this.Data);
		this.services.Add(this.AutoNotify);
		this.services.Add(this.Library);
		this.services.Add(this.ActorLifecycle);
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
	public TargetService Targets { get; init; } = new();
	public PanelService Panels { get; init; } = new();
	public GameDataService Data { get; init; } = new();
	public LibraryService Library { get; init; } = new();
	public ActorLifecycleService ActorLifecycle { get; init; } = new();

	/// <summary>
	/// Initialize and Start all services.
	/// </summary>
	public async Task Start()
	{
		foreach(ServiceBase service in this.services)
		{
			await service.Initialize();
		}

		foreach (ServiceBase service in this.services)
		{
			await service.Start();
		}
	}

	/// <summary>
	/// Stop and shutdown all services.
	/// </summary>
	public async Task Stop()
	{
		foreach (ServiceBase service in this.services)
		{
			await service.Stop();
		}

		foreach (ServiceBase service in this.services)
		{
			await service.Shutdown();
		}
	}
}

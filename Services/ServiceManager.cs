// © Anamnesis.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using System;
using System.Threading.Tasks;
using Serilog;
using XivToolsWpf;
using System.Diagnostics;
using System.Collections.Generic;
using ScreenshotStudio;


public class ServiceManager
{
	private readonly List<ServiceBase> services = new();

	public ServiceManager()
	{
		Instance = this;
	}

	public static ServiceManager Instance { get; private set; } = null!;

	public double BootProgress { get; private set; } = 0;
	public bool BootComplete { get; private set; } = false;

	public SettingsService Settings { get; } = new();
	public PanelService Panels { get; } = new();

	public async Task InitializeCriticalServices()
	{
		await this.InitializeService(this.Settings);
	}

	public async Task InitializeServices()
	{
		await this.InitializeService(this.Panels);

		await this.StartServices();
		this.BootComplete = true;
	}

	public async Task StartServices()
	{
		foreach (ServiceBase service in this.services)
		{
			Stopwatch sw = new();
			sw.Start();
			await service.Start();
			Log.Information($"Started service: {service.GetType().Name} in {sw.ElapsedMilliseconds}ms");
		}
	}

	public async Task ShutdownServices()
	{
		// shutdown services in reverse order
		this.services.Reverse();

		foreach (ServiceBase service in this.services)
		{
			try
			{
				// If this throws an exception we should keep trying to shut down the rest
				// not doing so can leave the game memory in a corrupt state
				await service.Shutdown();
			}
			catch (Exception ex)
			{
				Log.Error(ex, "Failed to shutdown service.");
			}
		}
	}

	private async Task InitializeService(ServiceBase service)
	{
		Stopwatch sw = new();
		sw.Start();
		await service.Initialize();
		this.services.Add(service);
		Log.Information($"Initialized service: {service.GetType().Name} in {sw.ElapsedMilliseconds}ms");
	}
}

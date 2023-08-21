// © XivTools.
// Licensed under the MIT license.

namespace ScreenshotStudio.Services;

using ScreenshotStudio.Structs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XivToolsWpf.Extensions;

public class StructViewModelService : ServiceBase
{
	private static readonly HashSet<StructViewModelBase> ViewModels = new();

	public static void Register(StructViewModelBase vm)
	{
		lock (ViewModels)
		{
			ViewModels.Add(vm);
		}
	}

	public static void Unregister(StructViewModelBase vm)
	{
		lock (ViewModels)
		{
			ViewModels.Remove(vm);
		}

		Serilog.Log.Information($"Lost VM {vm}");
	}

	public override async Task Start()
	{
		await base.Start();
		this.TickStructs().Run();
	}

	private async Task TickStructs()
	{
		HashSet<StructViewModelBase> viewModels;
		while(this.IsAlive)
		{
			try
			{
				await Task.Delay(33);

				lock (ViewModels)
				{
					viewModels = new(ViewModels);
				}

				foreach (StructViewModelBase vm in viewModels)
				{
					if (vm.IsDisposed)
						continue;

					vm.Tick();
				}
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error while ticking struct view models");
			}
		}
	}
}

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

namespace StudioFourteen.Overlays.Primitives;
using Serilog;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfUtils;
using WpfUtils.Extensions;

public class PrimitiveRenderer : Canvas
{
	public readonly List<IPrimitive> Primitives = new();

	public PrimitiveRenderer()
	{
		this.IsVisibleChanged += this.OnIsVisibleChanged;
	}

	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public void AddPrimitive(IPrimitive primitive)
	{
		this.Dispatcher.Invoke(() =>
		{
			primitive.Enable(this);
		});

		this.Primitives.Add(primitive);
	}

	public void RemovePrimitive(IPrimitive primitive)
	{
		this.Dispatcher.Invoke(() =>
		{
			primitive.Disable(this);
		});

		this.Primitives.Remove(primitive);
	}

	protected virtual Matrix4x4 GetViewMatrix() => this.Services.Camera.CurrentView;
	protected virtual Matrix4x4 GetProjectionMatrix() => this.Services.Camera.CurrentProjection;

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible)
		{
			this.RenderTask().Run();
		}
	}

	private async Task RenderTask()
	{
		try
		{
			await this.MainThread();
			while (this.IsVisible && !ServiceManager.ShutdownRequested)
			{
				await Task.Delay(10);
				await this.MainThread();

				if (!this.IsVisible || ServiceManager.ShutdownRequested)
					return;

				for (int i = this.Primitives.Count - 1; i >= 0; i--)
				{
					IPrimitive primitive = this.Primitives[i];

					try
					{
						primitive.Update(this.GetViewMatrix(), this.GetProjectionMatrix());
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Error in primitive transform {primitive}");
					}
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in primitive renderer");
		}
	}
}

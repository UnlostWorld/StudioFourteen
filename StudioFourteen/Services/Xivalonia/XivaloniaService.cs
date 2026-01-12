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

namespace StudioFourteen.Services.Xivalonia;

using System;
using System.Threading;
using Avalonia;
using StudioFourteen.Services.Xivalonia.Platform;
using Avalonia.Controls;

public partial class XivaloniaService : IService
{
	private readonly CancellationTokenSource cts = new();
	private readonly Thread? uiThread;

	public XivaloniaService()
	{
		ThreadStart ts = new(this.StartImpl);
		this.uiThread = new Thread(ts);
		this.uiThread.Start();
	}

	public void Dispose()
	{
		XivaloniaPlatform.Stop();
		this.cts.CancelAfter(250);
	}

	private void StartImpl()
	{
		try
		{
			while (Studio.Rendering.OverlayRenderer.BackBuffer == null)
			{
				Thread.Sleep(1000);
			}

			Studio.Log.Information($"setting up Xivalonia");

			////Avalonia.Logging.Logger.Sink

			AppBuilder app = AppBuilder.Configure<App>();
			app.WithInterFont();
			app.LogToTrace();

			bool useWin32 = false;

			if (useWin32)
			{
				app.With<Win32PlatformOptions>(() =>
				{
					return new()
					{
						CompositionMode = [Win32CompositionMode.LowLatencyDxgiSwapChain],
					};
				});

				app.UseWin32();
			}
			else
			{
				app.UseStandardRuntimePlatformSubsystem();
				app.UseWindowingSubsystem(() => XivaloniaPlatform.Initialize(), "Xivalonia");
			}

			app.UseSkia();

			Studio.Log.Information($"Starting Xivalonia");

			string[] args = [];
			app.Start(
				(app2, args) =>
				{
					// Ready to run!
					MainWindow mainWindow = new();
					mainWindow.Show();

					app2.Run(this.cts.Token);

					Studio.Log.Information($"Bye!");
				},
				args);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "error in main");
		}
	}
}
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

namespace StudioFourteen.Services.Avalonia;

using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using global::Avalonia;
using global::Avalonia.Rendering;
using global::Avalonia.Rendering.Composition;
using global::Avalonia.Threading;

public class RenderTimer : IRenderTimer, IDisposable
{
	private readonly Timer timer;
	private readonly Stopwatch stopwatch;
	private bool isDisposed;

	public RenderTimer(TimeSpan frameTime)
	{
		this.timer = new Timer(this.DoTick, null, frameTime, frameTime);
		this.stopwatch = Stopwatch.StartNew();
	}

	public event Action<TimeSpan>? Tick;
	public bool RunsInBackground => true;

	public void Dispose()
	{
		this.isDisposed = true;
		this.timer.Dispose();
		this.stopwatch.Stop();
	}

	private void DoTick(object? state)
	{
		try
		{
			if (this.isDisposed)
				return;

			this.Tick?.Invoke(this.stopwatch.Elapsed);
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error in Avalonia Render");
		}
	}
}
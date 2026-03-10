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

namespace StudioFourteen.Interface;

using System;
using StudioFourteen.Services.Avalonia;
using StudioFourteen.Services.Tick;

public class InterfaceService : IDisposable
{
	private readonly Hierarchy hierarchy = new();
	private readonly Inspector inspector = new();

	private bool isWaitingForReady = false;

	public InterfaceService()
	{
		Studio.Avalonia.Ready += this.OnAvaloniaReady;
	}

	public void Open()
	{
		if (!Studio.Avalonia.IsReady)
		{
			this.isWaitingForReady = true;
			return;
		}

		this.isWaitingForReady = false;

		Studio.Tick.Dispatch(TickChannels.Ui, () =>
		{
			this.hierarchy.Show();
		});
	}

	public void Close()
	{
		this.hierarchy.Close();
	}

	public void Dispose()
	{
		this.hierarchy.Dispose();
		this.inspector.Dispose();
	}

	private void OnAvaloniaReady(AvaloniaService service)
	{
		try
		{
			if (this.isWaitingForReady)
			{
				this.Open();
			}
		}
		catch (Exception ex)
		{
			Studio.Log.Error(ex, "Error showing initial hierarchy window");
		}
	}
}
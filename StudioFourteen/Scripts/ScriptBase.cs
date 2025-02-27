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

namespace StudioFourteen.Scripts;

using System;
using System.Threading.Tasks;
using Serilog;
using StudioFourteen.Studio;

public abstract class ScriptBase
{
	protected readonly ILogger Log;

	private LongTaskWindow? longTaskWindow;

	public ScriptBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public async Task RunScript()
	{
		this.longTaskWindow = await LongTaskWindow.Show();
		try
		{
			await this.Run();
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, "Error in script");
		}

		this.longTaskWindow?.Close();
	}

	protected abstract Task Run();

	protected void SetStatus(string str)
	{
		this.longTaskWindow?.SetStatus(str);
	}
}
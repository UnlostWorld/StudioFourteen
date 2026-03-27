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

namespace StudioFourteen.Services.Platform;

using System;
using System.Runtime.InteropServices;

public class PlatformService : IService
{
	public PlatformService()
	{
		this.OperatingSystem = global::Dalamud.Utility.Util.GetHostPlatform();
		if (this.OperatingSystem == OSPlatform.Linux)
		{
			if (Environment.GetEnvironmentVariable("DXMT_CONFIG") != null)
			{
				this.OperatingSystem = OSPlatform.OSX;
			}
		}

		Studio.Log.Information($"Platform OS: {this.OperatingSystem}");
	}

	public OSPlatform OperatingSystem { get; private set; }

	public void Dispose()
	{
	}
}
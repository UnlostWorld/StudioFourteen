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

namespace StudioFourteen.Launcher;

using System.Diagnostics;
using XIVLauncher.Common;
using XIVLauncher.Common.Dalamud;
using XIVLauncher.Common.PlatformAbstractions;
using XIVLauncher.Common.Windows;
using XIVLauncher.PlatformAbstractions;

internal class Program
{
	private static async Task Main(string[] args)
	{
		Console.WriteLine("Starting...");
		DirectoryInfo gameDir = new("C:/Program Files (x86)/Steam/steamapps/common/FINAL FANTASY XIV Online/");
		FileInfo exePath = new(Path.Combine(gameDir.FullName, "game", "ffxiv_dx11.exe"));
		Dictionary<string, string> environment = new();

		ConsoleOverlay overlay = new();

		CommonUniqueIdCache uniqueIdCache = new(new FileInfo(Path.Combine(Paths.RoamingPath, "uidCache.json")));

		DalamudUpdater dalamudUpdater = new(
			new DirectoryInfo(Path.Combine(Paths.RoamingPath, "addon")),
			new DirectoryInfo(Path.Combine(Paths.RoamingPath, "runtime")),
			new DirectoryInfo(Path.Combine(Paths.RoamingPath, "dalamudAssets")),
			new DirectoryInfo(Paths.RoamingPath),
			uniqueIdCache,
			string.Empty);

		dalamudUpdater.Overlay = overlay;

		DalamudLauncher dalamudLauncher = new(
			new WindowsDalamudRunner(),
			dalamudUpdater,
			DalamudLoadMethod.DllInject,
			gameDir,
			new DirectoryInfo(Paths.RoamingPath),
			new DirectoryInfo(Paths.RoamingPath),
			ClientLanguage.English,
			0,
			true,
			false,
			false,
			string.Empty);

		dalamudUpdater.Run(null, null);

		await Task.Run(() =>
		{
			DalamudLauncher.DalamudInstallState state = dalamudLauncher.HoldForUpdate(gameDir);
			Console.WriteLine($"Dalamud install: {state}");
		});

		Console.WriteLine("Launch...");

		dalamudLauncher.Run(exePath, "--S14", environment);
	}

	private class ConsoleOverlay : IDalamudLoadingOverlay
	{
		public void ReportProgress(long? size, long downloaded, double? progress)
		{
			if (size == null || progress == null)
				return;

			Console.WriteLine($"Downloading {size} / {downloaded} bytes. {progress * 100}%");
		}

		public void SetStep(IDalamudLoadingOverlay.DalamudUpdateStep step)
			=> Console.WriteLine($"> {step}");

		public void SetVisible()
			=> Console.WriteLine($"Dalamud Update...");

		public void SetInvisible()
			=> Console.WriteLine($"Dalamud Update done.");
	}
}
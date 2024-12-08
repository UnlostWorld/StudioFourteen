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

// Dalamud
// https://github.com/goatcorp/Dalamud/blob/master/Dalamud/Interface/Internal/ReShadeHandling/
namespace StudioFourteen.Reshade;

using StudioFourteen.Services;
using System.Runtime.CompilerServices;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Serilog.Events;

public class ReshadeService : ServiceBase
{
	private readonly LogDelegate addOnLogDelegate;

	public ReshadeService()
	{
		this.addOnLogDelegate = new LogDelegate(this.OnAddOnLog);
	}

	public delegate void LogDelegate(LogEventLevel logLevel, string message);

	// TODO: Check the current reshade version and warn the user if
	// the version is too old for us to communicate with.
	// also don't attempt to initialize the addon if we know its too old.
	// Also, GShade users still exist, we should check against that?
	public override void Attach()
	{
		IntPtr onLog = Marshal.GetFunctionPointerForDelegate(this.addOnLogDelegate);

		bool result = InitializeReshadeAddon(onLog);

		if (!result)
			this.Log.Error("Error initializing reshade addon");

		base.Attach();
	}

	public override void Detach()
	{
		ShutdownReshadeAddon();
		base.Detach();
	}

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Initialize")]
	private static extern bool InitializeReshadeAddon(IntPtr onLog);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Shutdown")]
	private static extern void ShutdownReshadeAddon();

	private void OnAddOnLog(LogEventLevel logLevel, string message)
	{
		this.Log.Write(logLevel, message);
	}
}
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

namespace StudioFourteen.Reshade;

using StudioFourteen.Services;
using System;
using System.Runtime.InteropServices;
using Serilog.Events;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Windows.Win32;

public class ReshadeService : ServiceBase
{
	private readonly LogDelegate onLog;
	private readonly OpenOverlayDelegate onOpenOverlay;
	private readonly SetCurrentPresetPathDelegate onSetCurrentPresetPath;

	public ReshadeService()
	{
		this.onLog = new LogDelegate(this.OnLog);
		this.onOpenOverlay = new OpenOverlayDelegate(this.OnOpenOverlay);
		this.onSetCurrentPresetPath = new SetCurrentPresetPathDelegate(this.OnSetCurrentPresetPath);
	}

	public delegate void ReshadeOverlayChangedDelegate(bool open);

	private delegate void LogDelegate(LogEventLevel logLevel, string message);
	private delegate bool OpenOverlayDelegate(IntPtr pEffectRuntime, bool open, int inputSource);
	private delegate void SetCurrentPresetPathDelegate(IntPtr pEffectRuntime, string path);

	public event ReshadeOverlayChangedDelegate? ReshadeOverlayChanged;

	public enum AddonEvents : uint
	{
		SetCurrentPresetPath = 84,
		OpenOverlay = 86,
	}

	public bool IsReshade { get; private set; }
	public bool IsReshadeOverlayOpen { get; set; }

	public override async Task Start()
	{
		await base.Start();

		string? dxgiPath = this.Services.Windows.XivProcess?.MainModule?.FileName;
		if (dxgiPath == null)
			throw new Exception("Failed to get xiv process path");

		dxgiPath = Path.GetDirectoryName(dxgiPath) + "/dxgi.dll";

		if (!File.Exists(dxgiPath))
			return;

		FileVersionInfo version = FileVersionInfo.GetVersionInfo(dxgiPath);
		int versionPacked = (version.ProductMajorPart * 10000) + (version.ProductMinorPart * 100) + version.ProductBuildPart;

		// 6.3.0 becomes 60300
		if (versionPacked < 60300)
		{
			// TODO: Replace this with a message dialog that can be disabled.
			this.Log.Error("Outdated Reshade install. Only version 6.3.0 or newer is supported. Please update reshade.");
			return;
		}

		this.Log.Information($"Reshade {version.ProductMajorPart}.{version.ProductMinorPart}.{version.ProductBuildPart} found and supported.");
		this.IsReshade = true;
	}

	// TODO: Check the current reshade version and warn the user if
	// the version is too old for us to communicate with.
	// also don't attempt to initialize the addon if we know its too old.
	// Also, GShade users still exist, we should check against that?
	public override void Attach()
	{
		base.Attach();

		if (!this.IsReshade)
			return;

		bool result = InitializeReshadeAddon(Marshal.GetFunctionPointerForDelegate(this.onLog));

		if (!result)
			this.Log.Error("Error initializing reshade add-on");

		this.Log.Information("Initialized Reshade add-on");

		RegisterEvent(AddonEvents.OpenOverlay, Marshal.GetFunctionPointerForDelegate(this.onOpenOverlay));
		RegisterEvent(AddonEvents.SetCurrentPresetPath, Marshal.GetFunctionPointerForDelegate(this.onSetCurrentPresetPath));
	}

	public override void Detach()
	{
		base.Detach();

		if (!this.IsReshade)
			return;

		UnregisterEvent(AddonEvents.OpenOverlay, Marshal.GetFunctionPointerForDelegate(this.onOpenOverlay));
		UnregisterEvent(AddonEvents.SetCurrentPresetPath, Marshal.GetFunctionPointerForDelegate(this.onSetCurrentPresetPath));
		ShutdownReshadeAddon();
	}

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Initialize")]
	private static extern bool InitializeReshadeAddon(IntPtr onLog);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Shutdown")]
	private static extern void ShutdownReshadeAddon();

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "RegisterEvent")]
	private static extern void RegisterEvent(AddonEvents evt, IntPtr callback);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "UnregisterEvent")]
	private static extern void UnregisterEvent(AddonEvents evt, IntPtr callback);

	private void OnLog(LogEventLevel logLevel, string message) => this.Log.Write(logLevel, message);

	private bool OnOpenOverlay(IntPtr pEffectRuntime, bool open, int inputSource)
	{
		this.IsReshadeOverlayOpen = open;
		this.RaisePropertyChanged(nameof(this.IsReshadeOverlayOpen));
		this.ReshadeOverlayChanged?.Invoke(open);

		// We can stop the overlay from opening by returning true.
		// We may want to do this if studio will have its own reshade UI.
		return false;
	}

	private void OnSetCurrentPresetPath(IntPtr pEffectRuntime, string path)
	{
		this.Log.Information($"Reshade preset changed: {path}");
	}
}
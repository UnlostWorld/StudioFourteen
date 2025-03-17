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

using Dalamud.Plugin.Services;
using Serilog.Events;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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
	public delegate void ReshadeDelegate();

	private delegate void LogDelegate(LogEventLevel logLevel, string message);
	private delegate bool OpenOverlayDelegate(IntPtr pEffectRuntime, bool open, int inputSource);
	private delegate void SetCurrentPresetPathDelegate(IntPtr pEffectRuntime, string path);
	private delegate void EffectRuntimeDelegate(IntPtr pEffectRuntime);

	public event ReshadeOverlayChangedDelegate? ReshadeOverlayChanged;

	public enum AddonEvents : uint
	{
		ReshadePresent = 75,
		ReshadeReloadedEffects = 78,
		SetCurrentPresetPath = 84,
		OpenOverlay = 86,
	}

	public bool IsReshade { get; private set; }
	public bool IsReshadeOverlayOpen { get; set; }

	public IntPtr DepthBufferAddress { get; private set; }

	public override async Task Start()
	{
		await base.Start();

		if (!DalamudServices.IsAlive)
			return;

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
			this.Log.Information($"Reshade {version.ProductMajorPart}.{version.ProductMinorPart}.{version.ProductBuildPart} found.");

			if (this.Settings.HasConfirmedReShadeVersion != versionPacked)
			{
				NotSupportedPanel? nsp = await ServiceManager.Instance.Panels.Open<NotSupportedPanel>();
				if (nsp != null)
				{
					nsp.ReShadeVersionPacked = versionPacked;
				}
			}

			return;
		}

		this.Log.Information($"Reshade {version.ProductMajorPart}.{version.ProductMinorPart}.{version.ProductBuildPart} found and supported.");
		this.IsReshade = true;
	}

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

	public async Task<bool> WaitForEffectsToLoad(long timeout = 60_000)
	{
		if (!this.IsReshade || !this.IsAttached)
			return true;

		Stopwatch sw = new();
		sw.Start();

		ResetRenderedFrames();

		while(GetRenderedFrames() < 60 * 2
			&& sw.ElapsedMilliseconds < timeout)
		{
			await Task.Delay(500);
		}

		// Some extra time for auto-focus and adaption effects to do their thing.
		await Task.Delay(5000);

		bool timedOut = sw.ElapsedMilliseconds > timeout;
		if (timedOut)
			this.Log.Error("Timeout waiting for effects to load");

		return !timedOut;
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		this.DepthBufferAddress = GetDepthTexture();
		base.OnFrameworkUpdate(framework);
	}

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Initialize")]
	private static extern bool InitializeReshadeAddon(IntPtr onLog);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "Shutdown")]
	private static extern void ShutdownReshadeAddon();

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "RegisterEvent")]
	private static extern void RegisterEvent(AddonEvents evt, IntPtr callback);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "UnregisterEvent")]
	private static extern void UnregisterEvent(AddonEvents evt, IntPtr callback);

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "GetDepthTexture")]
	private static extern IntPtr GetDepthTexture();

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "ResetRenderedFrames")]
	private static extern int ResetRenderedFrames();

	[DllImport("StudioFourteen.Reshade.dll", EntryPoint = "GetRenderedFrames")]
	private static extern int GetRenderedFrames();

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
	}
}
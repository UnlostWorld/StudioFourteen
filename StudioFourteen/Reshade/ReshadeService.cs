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
using PropertyChanged.SourceGenerator;
using Serilog.Events;
using StudioFourteen.Plugin;
using StudioFourteen.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

public static class ReshadeAddon
{
	[DllImport("StudioFourteen.Reshade.dll")] public static extern bool Initialize(IntPtr onLog);
	[DllImport("StudioFourteen.Reshade.dll")] public static extern void Shutdown();
	[DllImport("StudioFourteen.Reshade.dll")] public static extern int ResetRenderedFrames();
	[DllImport("StudioFourteen.Reshade.dll")] public static extern int GetRenderedFrames();
	[DllImport("StudioFourteen.Reshade.dll")] public static extern bool GetEffectsState();
	[DllImport("StudioFourteen.Reshade.dll")] public static extern void SetEffectsState(bool state);
	[DllImport("StudioFourteen.Reshade.dll")] public static extern bool GetIsOverlayOpen();
	[DllImport("StudioFourteen.Reshade.dll")] public static extern bool SetBeginRenderingEffectsCallback(IntPtr callback);
	[DllImport("StudioFourteen.Reshade.dll")] public static extern bool SetFinishRenderingEffectsCallback(IntPtr callback);
}

public partial class ReshadeService : ServiceBase
{
	private readonly LogDelegate onLog;
	private readonly EmptyDelegate onBeginRenderingEffects;
	private readonly EmptyDelegate onFinishRenderingEffects;

	[Notify] private bool isReshadeOverlayOpen;
	[Notify] private bool isReshadeEnabled;

	public ReshadeService()
	{
		this.onLog = new LogDelegate(this.OnLog);
		this.onBeginRenderingEffects = new EmptyDelegate(this.OnBeginRenderingEffects);
		this.onFinishRenderingEffects = new EmptyDelegate(this.OnFinishRenderingEffects);
	}

	public delegate void EmptyDelegate();
	public delegate void ReshadeOverlayChangedDelegate(bool open);
	private delegate void LogDelegate(LogEventLevel logLevel, string message);

	public event ReshadeOverlayChangedDelegate? ReshadeOverlayChanged;
	public event EmptyDelegate? ReshadeBeforeEffects;
	public event EmptyDelegate? ReshadeAfterEffects;

	public bool IsReshade { get; private set; }

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

		// 6.5.0 becomes 60500
		if (versionPacked < 60500)
		{
			this.Log.Information($"Reshade {version.ProductMajorPart}.{version.ProductMinorPart}.{version.ProductBuildPart} found.");

			if (this.Settings.HasConfirmedReShadeVersion != versionPacked)
			{
				NotSupportedPanel? nsp = await ServiceManager.Instance.Panels.GamePanels.CreatePanelAsync<NotSupportedPanel>(true);
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

		this.Services.Tick.Add(TickService.Channels.StudioTick, this.OnTick);

		ReshadeAddon.SetBeginRenderingEffectsCallback(Marshal.GetFunctionPointerForDelegate(this.onBeginRenderingEffects));
		ReshadeAddon.SetFinishRenderingEffectsCallback(Marshal.GetFunctionPointerForDelegate(this.onFinishRenderingEffects));
		bool result = ReshadeAddon.Initialize(Marshal.GetFunctionPointerForDelegate(this.onLog));

		if (!result)
			this.Log.Error("Error initializing reshade add-on");

		this.Log.Information("Initialized Reshade add-on");
	}

	public override void Detach()
	{
		base.Detach();

		if (!this.IsReshade)
			return;

		this.Services.Tick.Remove(TickService.Channels.StudioTick, this.OnTick);
		ReshadeAddon.Shutdown();
	}

	public async Task<bool> WaitForEffectsToLoad(long timeout = 60_000)
	{
		if (!this.IsReshade || !this.IsAttached || !this.IsReshadeEnabled)
			return true;

		Stopwatch sw = new();
		sw.Start();

		ReshadeAddon.ResetRenderedFrames();

		while(ReshadeAddon.GetRenderedFrames() < 60 * 2
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

	protected void OnTick()
	{
		this.IsReshadeOverlayOpen = ReshadeAddon.GetIsOverlayOpen();
		this.IsReshadeEnabled = ReshadeAddon.GetEffectsState();
	}

	protected void OnIsReshadeOverlayOpenChanged(bool oldValue, bool newValue)
	{
		this.ReshadeOverlayChanged?.Invoke(newValue);
	}

	protected void OnIsReshadeEnabledChanged(bool oldValue, bool newValue)
	{
		ReshadeAddon.SetEffectsState(newValue);
	}

	private void OnLog(LogEventLevel logLevel, string message) => this.Log.Write(logLevel, message);

	private void OnBeginRenderingEffects()
	{
		this.ReshadeBeforeEffects?.Invoke();
	}

	private void OnFinishRenderingEffects()
	{
		this.ReshadeAfterEffects?.Invoke();
	}
}
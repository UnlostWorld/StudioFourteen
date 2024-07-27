namespace ScreenshotStudio;

using ScreenshotStudio.Data;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Input;
using ScreenshotStudio.Library;
using ScreenshotStudio.Services;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Utilities;
using System.Diagnostics;

public class ServiceManager : ServiceManagerBase
{
	public static new ServiceManager Instance => (ServiceManager)ServiceManagerBase.Instance;

	public InteropService Interop { get; init; } = new();
	public AutoPropertyNotifyService AutoNotify { get; init; } = new();
	public SettingsService Settings { get; init; } = new();
	public PanelService Panels { get; init; } = new();
	public StudioService Studio { get; init; } = new();
	public DataService Data { get; init; } = new();
	public GameDataService GameData { get; init; } = new();
	public CharacterLifecycleService CharacterLifecycle { get; init; } = new();
	public GroupPoseService GroupPose { get; init; } = new();
	public CharacterAppearanceService CharacterAppearance { get; init; } = new();
	public LibraryService Library { get; init; } = new();
	public InputService Input { get; init; } = new();
	public GameCaptureService GameCapture { get; init; } = new();
	public PoseService Pose { get; init; } = new();

	protected override void OnStart()
	{
		Logging.Init();

		// Hard reference our required satellite assemblies to make sure dalamuds plugin loader picks them up.
		this.Log.Information($"Ensure assembly XivToolWpf {typeof(WpfUtils.Dispatch).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome {typeof(FontAwesome.Sharp.Icon).Assembly}");
		this.Log.Information($"Ensure assembly FontAwesome Pro {typeof(FontAwesome.Sharp.Pro.Icon).Assembly}");
		this.Log.Information($"Ensure assembly VirtualizingWrapPanel Pro {typeof(WpfToolkit.Controls.VirtualizingWrapPanel).Assembly}");

		// Get the Xiv process for window manipulation.
		// NOTE: if we _don't_ log out the value here, then things break. I don't know why.
		XivWindow.Process = Process.GetCurrentProcess();
		this.Log.Information($"Ensure XivProcess {XivWindow.Process} - {XivWindow.Process.MainWindowHandle} - {XivWindow.Process.MainWindowTitle}");

		Alloc.Init();
	}

	protected override void OnStop()
	{
		Alloc.Dispose();
	}
}
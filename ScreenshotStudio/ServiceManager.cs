namespace ScreenshotStudio;

using ScreenshotStudio.Data;
using ScreenshotStudio.Files;
using ScreenshotStudio.GameData;
using ScreenshotStudio.Input;
using ScreenshotStudio.Library;
using ScreenshotStudio.Posing;
using ScreenshotStudio.Services;
using ScreenshotStudio.Tablet;

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
	public FileThumbnailService Thumbnails { get; init; } = new();
	public TargetService Target { get; init; } = new();
	public TabletService Tablet { get; init; } = new();
}
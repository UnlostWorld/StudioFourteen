namespace StudioFourteen;

using StudioFourteen.Appearance;
using StudioFourteen.Cameras;
using StudioFourteen.Data;
using StudioFourteen.Files;
using StudioFourteen.GameData;
using StudioFourteen.Input;
using StudioFourteen.IPC;
using StudioFourteen.Library;
using StudioFourteen.Mvm;
using StudioFourteen.Online;
using StudioFourteen.Overlays;
using StudioFourteen.Posing;
using StudioFourteen.Save;
using StudioFourteen.Services;
using StudioFourteen.Settings;
using StudioFourteen.Tablet;

public class ServiceManager : ServiceManagerBase
{
	public static new ServiceManager Instance => (ServiceManager)ServiceManagerBase.Instance;

	public OnlineService Online { get; init; } = new();
	public InteropService Interop { get; init; } = new();
	public AutoPropertyNotifyService AutoNotify { get; init; } = new();
	public WindowService Windows { get; init; } = new();
	public SettingsService Settings { get; init; } = new();
	public FramerateService Framerate { get; init; } = new();
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
	public SaveService Save { get; init; } = new();
	public ActorRoleService Roles { get; init; } = new();
	public FileService Files { get; init; } = new();
	public PanelService Panels { get; init; } = new();
	public CameraService Camera { get; init; } = new();
	public OverlayService Overlays { get; init; } = new();
	public EnvironmentService Environment { get; init; } = new();
	public GameConfigService GameConfiguration { get; init; } = new();
	public IPCService IPC { get; init; } = new();
}
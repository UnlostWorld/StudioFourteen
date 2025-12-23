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

namespace StudioFourteen;

using StudioFourteen.Avalonia;
using StudioFourteen.Services;

public class ServiceManager : ServiceManagerBase
{
	public static new ServiceManager Instance => (ServiceManager)ServiceManagerBase.Instance;

	public Online.OnlineService Online { get; init; } = new();
	public Settings.SettingsService Settings { get; init; } = new();
	public StudioService Studio { get; init; } = new();
	public GameData.GameDataService GameData { get; init; } = new();
	public CharacterLifecycleService CharacterLifecycle { get; init; } = new();
	public GroupPoseService GroupPose { get; init; } = new();
	public Appearance.CharacterAppearanceService CharacterAppearance { get; init; } = new();
	public Library.LibraryService Library { get; init; } = new();
	public Input.InputService Input { get; init; } = new();
	public Files.FileThumbnailService Thumbnails { get; init; } = new();
	public Tablet.TabletService Tablet { get; init; } = new();
	public Files.FileService Files { get; init; } = new();
	public Scene.Cameras.CameraService Camera { get; init; } = new();
	public IPC.IPCService IPC { get; init; } = new();
	public Reshade.ReshadeService Reshade { get; init; } = new();
	public Selection.SelectionService Selection { get; init; } = new();
	public Photos.PhotosService Photos { get; init; } = new();
	public RedrawService Redraw { get; init; } = new();
	public Analytics.ErrorReportingService Errors { get; init; } = new();
	public TickService Tick { get; init; } = new();
	public Animation.AnimationService Animations { get; init; } = new();
	public DragAndDrop.DragAndDropService DragAndDrop { get; init; } = new();
	public Environment.EnvironmentService Environment { get; init; } = new();
	public Environment.TerritoryService Territory { get; init; } = new();
	public Environment.TimeService Time { get; init; } = new();
	public Rendering.RenderingService Rendering { get; init; } = new();
	public Rendering.Draw.Gizmos.GizmoService Gizmos { get; init; } = new();
	public Content.ContentService Content { get; init; } = new();
	public AfkService Afk { get; init; } = new();
	public WindowService Windows { get; init; } = new();
	public AvaloniaService Avalonia { get; init; } = new();

	public Scene.SceneService Scene { get; init; } = new();
	public Scene.GameObjects.GameObjectService GameObjects { get; init; } = new();
	public Scene.GameObjects.Characters.Skeletons.SkeletonService Skeletons { get; init; } = new();
}
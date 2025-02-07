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

namespace StudioFourteen.Selection;

using Dalamud.Plugin.Services;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using System.Threading.Tasks;

public partial class SelectionService : ServiceBase
{
	private readonly TransformHandleOverlayLayer poseGizmoOverlay = new();
	private SelectionBase? selection;

	[Notify]
	[AlsoNotify(nameof(SelectionService.GizmoIndex))]
	private TransformHandleTypes gizmo = TransformHandleTypes.Rotation;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);

	public event SelectionChangedDelegate? SelectionChanged;

	public SelectionBase? Selection
	{
		get => this.selection;
		set
		{
			this.selection?.Deactivate();

			this.selection = value;

			if (this.selection != null)
			{
				this.selection.Activate();
			}

			if (this.selection is TransformSelectionBase transformSelection)
			{
				this.Gizmo = transformSelection.DefaultGizmo;
			}

			this.SelectionChanged?.Invoke(value);
			this.RaisePropertyChanged();

			this.poseGizmoOverlay.SetSelection(this.selection);
		}
	}

	public int GizmoIndex
	{
		get => (int)this.Gizmo;
		set => this.Gizmo = (TransformHandleTypes)value;
	}

	public override Task Start()
	{
		this.Services.Target.TargetChanged += this.OnTargetChanged;
		return base.Start();
	}

	public override Task Stop()
	{
		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		return base.Stop();
	}

	public override void Attach()
	{
		base.Attach();

		this.poseGizmoOverlay.Enable();
	}

	public override void Detach()
	{
		base.Detach();

		this.poseGizmoOverlay.Disable();
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);
		this.Selection?.OnFrameworkUpdate(framework);
	}

	private void OnTargetChanged()
	{
		this.poseGizmoOverlay.SetTarget(this.Services.Target.TargetObjectIndex);

		// TODO: consider caching the previous selection this target had and restoring it?
		this.Selection = new GameObjectSelection(this.Services.Target.TargetObjectIndex);
	}
}

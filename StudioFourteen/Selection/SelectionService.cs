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
using FFXIVClientStructs.FFXIV.Common.Lua;
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.History;
using StudioFourteen.Posing;
using StudioFourteen.Services;
using System;
using System.Threading.Tasks;

public abstract class ISelectionId
{
	public abstract SelectionBase? Create();
}

public abstract class IAsyncSelectionId : ISelectionId
{
	public abstract Task<SelectionBase?> CreateAsync();

	public sealed override SelectionBase? Create() => throw new NotSupportedException();
}

public partial class SelectionService : ServiceBase, IHistoryTarget
{
	private readonly TransformHandleOverlayLayer poseGizmoOverlay = new();
	private SelectionBase? selection;

	[Notify]
	[AlsoNotify(nameof(SelectionService.GizmoIndex))]
	private TransformHandleTypes gizmo = TransformHandleTypes.Rotation;

	public delegate void SelectionChangedDelegate(SelectionBase? newSelection);

	public event SelectionChangedDelegate? SelectionChanged;

	public override string Name => "Selection";
	public override IconChar Icon => IconChar.MousePointer;

	public SelectionBase? Current
	{
		get => this.selection;
		set
		{
			string? oldSelectionName = this.selection?.Name;
			string? newSelectionName = value?.Name;
			this.Services.History.RecordChange(this, $"{oldSelectionName} -> {newSelectionName}");

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
		}
	}

	public int GizmoIndex
	{
		get => (int)this.Gizmo;
		set => this.Gizmo = (TransformHandleTypes)value;
	}

	[History]
	public ISelectionId? GetSelectionId()
	{
		return this.Current?.Id;
	}

	[History]
	public async Task SetSelectionId(ISelectionId? value)
	{
		if (value == null)
		{
			this.Current = null;
		}
		else if (value is IAsyncSelectionId asyncSelectionId)
		{
			this.Current = await asyncSelectionId.CreateAsync();
		}
		else
		{
			this.Current = value.Create();
		}
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
		this.Current?.OnFrameworkUpdate(framework);
	}

	private void OnTargetChanged(int objectTableIndex)
	{
		// TODO: consider caching the previous selection this target had and restoring it?
		this.Current = new GameObjectSelection(this.Services.Target.TargetObjectIndex);
	}
}

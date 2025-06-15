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

using StudioFourteen.History;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;
using StudioFourteen.Scene;
using StudioFourteen.Services;
using StudioFourteen.Widget;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;

public partial class SelectionService : ServiceBase
{
	private readonly List<SceneObjectGizmoBase> selectionGizmos = new();

	private SceneObjectBase? selection;
	private SceneObjectBase? hover;
	private string lastSelectionName = "Nothing";
	private bool expandedSelection;

	public delegate void SelectionChangedDelegate(SceneObjectBase? oldSelection, SceneObjectBase? newSelection, object? selectionSource);
	public delegate void SelectionExpandedDelegate(bool newValue);

	public event SelectionChangedDelegate? SelectionChanged;
	public event SelectionChangedDelegate? HoverChanged;
	public event SelectionExpandedDelegate? SelectionExpanded;

	public override string Name => "Selection";
	public override object? Icon => Resources.Find("ICON_Selection_SelectionService");

	public SceneObjectBase? Current => this.selection;
	public SceneObjectBase? Hover => this.hover;

	public bool ExpandedSelection
	{
		get => this.expandedSelection;
		set
		{
			if (this.expandedSelection == value)
				return;

			this.expandedSelection = value;
			this.SelectionExpanded?.Invoke(value);
		}
	}

	public object? HoverSource { get; set; }

	// The screen-position of the cursor then this selection was made.
	public Vector2 SelectionCursorPosition { get; set; }

	// An offset from where the cursor was and the transform root of the selected object (if it has one)
	public Vector2 SelectionCursorOffset { get; set; }

	public void Select(SceneObjectBase? newSelection, object? source)
	{
		SceneObjectBase? oldSelection = this.selection;

		if (oldSelection != null && newSelection != null && oldSelection.Id == newSelection.Id)
			return;

		this.lastSelectionName = this.selection?.Name ?? "Nothing";
		this.Services.History.RecordChange(this, $"Change");

		if (this.selection != null)
		{
			if (this.selection.IsActive)
				this.selection.Deactivate();

			this.selection.OnSelected(false);
		}

		this.selection = newSelection;
		this.selection?.OnSelected(true);

		if (this.selection != null && !this.selection.IsActive)
			this.selection.Activate();

		if (this.selection != null)
		{
			if (this.Services.Input.Mouse != null)
			{
				this.SelectionCursorPosition = this.Services.Input.Mouse.GetPosition();
				this.SelectionCursorOffset = Vector2.Zero;

				if (this.selection is TransformSceneObjectBase transformSelection)
				{
					Vector3 worldPos = Vector3.Transform(Vector3.Zero, transformSelection.WorldTransform.ToMatrix());
					Vector3 cameraPos = this.Services.Camera.WorldToCamera(worldPos);

					this.SelectionCursorOffset = this.SelectionCursorPosition - cameraPos.ToVector2();
				}
			}
		}

		this.Services.Handles.Timeout();
		this.SelectionChanged?.Invoke(oldSelection, newSelection, source);
		this.RaisePropertyChanged();
	}

	public void Clear()
	{
		this.Select(null, null);
	}

	public void HoverSelection(SceneObjectBase? newHover, object? source)
	{
		if (this.hover == newHover)
			return;

		SceneObjectBase? oldHover = this.hover;
		if (this.hover != null && this.hover != this.selection && this.hover.IsActive)
			this.hover.Deactivate();

		this.Hover?.OnHovered(false);
		this.hover = newHover;
		this.Hover?.OnHovered(true);

		if (this.hover != null && !this.hover.IsActive)
			this.hover.Activate();

		this.HoverChanged?.Invoke(oldHover, newHover, source);
		this.RaisePropertyChanged();
	}

	public void ClearHover()
	{
		this.HoverSelection(null, null);
	}

	public override void FinalizeHistoryOperation(ref Operation operation)
	{
		base.FinalizeHistoryOperation(ref operation);

		string? newSelectionName = this.selection?.Name;
		operation.Description = $"{this.lastSelectionName} > {newSelectionName}";
	}

	public override async Task Start()
	{
		await this.Services.Panels.GamePanels.SetIsOpenAsync<SelectionWidget>(true, false);

		this.selectionGizmos.Add(new SelectionGizmo());
		this.selectionGizmos.Add(new TranslationGizmo());
		this.selectionGizmos.Add(new RotationGizmo());
		this.selectionGizmos.Add(new ScaleGizmo());

		this.Services.Target.TargetChanged += this.OnTargetChanged;
		await base.Start();
	}

	public override async Task Stop()
	{
		await this.Services.Panels.GamePanels.SetIsOpenAsync<SelectionWidget>(false, false);

		this.Services.Target.TargetChanged -= this.OnTargetChanged;
		await base.Stop();
	}

	public override void Attach()
	{
		base.Attach();
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
	}

	public override void Detach()
	{
		base.Detach();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
	}

	protected void OnGameTick()
	{
		this.Current?.OnGameTick();

		/*if (!this.Services.Windows.IsCursorOverStudio)
		{
			HitInfo? hit = RayCast.CastFromCursor();
			if (hit != null)
			{
				if (this.Hover?.IsHit(hit) != true)
				{
					this.Hover = null;

					if (this.Current?.IsHit(hit) == true)
					{
						this.Hover = this.Current;
					}
					else
					{
						// TODO: somewhere else?
						if (hit.ObjectTableIndex != -1)
						{
							this.Hover = new ObjectTableSelection(hit.ObjectTableIndex);
						}
					}
				}
			}
		}*/

		if (this.Hover != this.Current)
		{
			this.Hover?.OnGameTick();
		}
	}

	private void OnTargetChanged(int objectTableIndex)
	{
		// TODO: consider caching the previous selection this target had and restoring it?
		this.Select(new ObjectTableObject(this.Services.Target.TargetObjectIndex), this);
	}
}

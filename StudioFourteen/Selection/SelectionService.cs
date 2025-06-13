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

using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.History;
using StudioFourteen.Posing;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Rendering.Draw.Gizmos.Transforms;
using StudioFourteen.Scene;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using StudioFourteen.Widgets;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using WpfUtils.Windows;

public partial class SelectionService : ServiceBase
{
	private readonly List<ObjectGizmoBase> selectionGizmos = new();

	private SceneObjectBase? selection;
	private SceneObjectBase? hover;
	private ObjectGizmoBase? gizmo;
	private string lastSelectionName = "Nothing";
	private bool expandedSelection;

	public delegate void SelectionChangedDelegate(SceneObjectBase? oldSelection, SceneObjectBase? newSelection);
	public delegate void GizmoChangedDelegate(ObjectGizmoBase? oldGizmo, ObjectGizmoBase? newGizmo);
	public delegate void SelectionExpandedDelegate(bool newValue);

	public event SelectionChangedDelegate? SelectionChanged;
	public event SelectionChangedDelegate? HoverChanged;
	public event GizmoChangedDelegate? GizmoChanged;
	public event SelectionExpandedDelegate? SelectionExpanded;

	public override string Name => "Selection";
	public override object? Icon => Resources.Find("ICON_Selection_SelectionService");

	public SceneObjectBase? Current
	{
		get => this.selection;
		set
		{
			SceneObjectBase? oldSelection = this.selection;

			if (oldSelection != null && value != null && oldSelection.Id == value.Id)
				return;

			this.lastSelectionName = this.selection?.Name ?? "Nothing";
			this.Services.History.RecordChange(this, $"Change");

			if (this.selection != null)
			{
				if (this.selection.IsActive)
					this.selection.Deactivate();

				this.selection.OnSelected(false);
			}

			this.selection = value;
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

			this.SelectionChanged?.Invoke(oldSelection, value);
			this.RaisePropertyChanged();

			this.DefaultGizmo();
		}
	}

	public SceneObjectBase? Hover
	{
		get => this.hover;
		set
		{
			if (this.hover == value)
				return;

			SceneObjectBase? oldHover = this.hover;
			if (this.hover != null && this.hover != this.selection && this.hover.IsActive)
				this.hover.Deactivate();

			this.Hover?.OnHovered(false);
			this.hover = value;
			this.Hover?.OnHovered(true);

			if (this.hover != null && !this.hover.IsActive)
				this.hover.Activate();

			this.HoverChanged?.Invoke(oldHover, value);
			this.RaisePropertyChanged();
		}
	}

	public ObjectGizmoBase? Gizmo
	{
		get => this.gizmo;
		set
		{
			ObjectGizmoBase? oldGizmo = this.gizmo;

			if (this.gizmo != null)
				this.gizmo.Disable();

			if (this.selection == null)
			{
				this.gizmo = null;
			}
			else
			{
				this.gizmo = value;
				this.gizmo?.Enable(this.selection);
			}

			this.RaisePropertyChanged();
			this.GizmoChanged?.Invoke(oldGizmo, this.gizmo);
			this.Services.Handles.Timeout();
		}
	}

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

	[History]
	public ISceneObjectId? GetSelectionId()
	{
		return this.Current?.Id;
	}

	[History]
	public async Task SetSelectionId(ISceneObjectId? value)
	{
		if (value == null)
		{
			this.Current = null;
		}
		else if (value is IAsyncSceneObjectId asyncSelectionId)
		{
			this.Current = await asyncSelectionId.CreateAsync();
		}
		else
		{
			this.Current = value.Create();
		}
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

	public List<ObjectGizmoBase> GetValidGizmos()
	{
		List<ObjectGizmoBase> results = new();

		if (this.selection == null)
			return results;

		foreach (ObjectGizmoBase gizmo in this.selectionGizmos)
		{
			if (!gizmo.SupportsObject(this.selection))
				continue;

			results.Add(gizmo);
		}

		return results;
	}

	protected void DefaultGizmo()
	{
		ObjectGizmoBase? nextGizmo = this.Gizmo;

		// TODO: Get the default gizmo from the selection actually.
		List<ObjectGizmoBase> results = this.GetValidGizmos();
		if (results.Count > 0)
		{
			if (nextGizmo == null || !results.Contains(nextGizmo))
			{
				nextGizmo = results[0];
			}
		}

		this.Gizmo = null;
		this.Gizmo = nextGizmo;
	}

	protected void OnGameTick()
	{
		this.Current?.OnGameTick();
		this.Gizmo?.OnGameTick();

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
		this.Current = new ObjectTableSelection(this.Services.Target.TargetObjectIndex);
	}
}

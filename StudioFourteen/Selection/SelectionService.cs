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
using FontAwesome.Sharp;
using PropertyChanged.SourceGenerator;
using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.History;
using StudioFourteen.Posing;
using StudioFourteen.Rendering.Gizmos;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

public abstract class ISelectionId : IEquatable<ISelectionId?>
{
	public static bool operator ==(ISelectionId? left, ISelectionId? right)
	{
		return EqualityComparer<ISelectionId>.Default.Equals(left, right);
	}

	public static bool operator !=(ISelectionId? left, ISelectionId? right)
	{
		return !(left == right);
	}

	public abstract SelectionBase? Create();

	public override bool Equals(object? obj)
	{
		return this.Equals(obj as ISelectionId);
	}

	public bool Equals(ISelectionId? other)
	{
		return other is not null && this.GetHashCode() == other.GetHashCode();
	}

	public override int GetHashCode()
	{
		throw new NotImplementedException();
	}
}

public abstract class IAsyncSelectionId : ISelectionId
{
	public abstract Task<SelectionBase?> CreateAsync();

	public sealed override SelectionBase? Create() => throw new NotSupportedException();
}

public partial class SelectionService : ServiceBase
{
	private readonly TransformHandleOverlayLayer poseGizmoOverlay = new();
	private readonly SelectionGizmo selectionGizmo = new();
	private SelectionBase? selection;
	private SelectionBase? hover;
	private string lastSelectionName = "Nothing";

	[Notify]
	[AlsoNotify(nameof(SelectionService.GizmoIndex))]
	private TransformHandleTypes gizmo = TransformHandleTypes.Rotation;

	public delegate void SelectionChangedDelegate(SelectionBase? oldSelection, SelectionBase? newSelection);

	public event SelectionChangedDelegate? SelectionChanged;
	public event SelectionChangedDelegate? HoverChanged;

	public override string Name => "Selection";
	public override IconChar Icon => IconChar.MousePointer;

	public SelectionBase? Current
	{
		get => this.selection;
		set
		{
			SelectionBase? oldSelection = this.selection;

			if (oldSelection != null && value != null && oldSelection.Id == value.Id)
				return;

			this.lastSelectionName = this.selection?.Name ?? "Nothing";
			this.Services.History.RecordChange(this, $"Change");

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

			this.SelectionChanged?.Invoke(oldSelection, value);
			this.RaisePropertyChanged();
		}
	}

	public SelectionBase? Hover
	{
		get => this.hover;
		set
		{
			if (this.hover == value)
				return;

			SelectionBase? oldHover = this.hover;
			oldHover?.Deactivate();
			this.hover = value;
			this.hover?.Activate();
			this.HoverChanged?.Invoke(oldHover, value);
			this.RaisePropertyChanged();
		}
	}

	public UIElement? HoverSource { get; set; }

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

	public override void FinalizeHistoryOperation(ref Operation operation)
	{
		base.FinalizeHistoryOperation(ref operation);

		string? newSelectionName = this.selection?.Name;
		operation.Description = $"{this.lastSelectionName} > {newSelectionName}";
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
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.poseGizmoOverlay.Enable();
		this.selectionGizmo.Enable();
	}

	public override void Detach()
	{
		base.Detach();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.poseGizmoOverlay.Disable();
		this.selectionGizmo.Disable();
	}

	protected void OnGameTick()
	{
		this.Current?.OnGameTick();

		if (!this.Services.Windows.IsCursorOverStudio)
		{
			SelectionBase? newHover = null;
			HitInfo? hit = RayCast.CastFromCursor();
			if (hit != null)
			{
				if (hit.ObjectTableIndex != -1)
				{
					if (this.Hover is ObjectTableSelection currentHover
						&& currentHover.ObjectTableId != hit.ObjectTableIndex)
					{
						newHover = this.Hover;
					}
					else
					{
						newHover = new ObjectTableSelection(hit.ObjectTableIndex);
					}
				}
			}

			this.Hover = newHover;
		}

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

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
using StudioFourteen.Rendering.Scene.Gizmos;
using StudioFourteen.Rendering.Scene.Gizmos.Transforms;
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
	private readonly List<SelectionGizmoBase> selectionGizmos = new();

	private SelectionBase? selection;
	private SelectionBase? hover;
	private SelectionGizmoBase? gizmo;
	private string lastSelectionName = "Nothing";

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

			if (this.selection != null)
			{
				if (this.selection.IsActive)
					this.selection.Deactivate();

				this.selection.OnSelected(false);
			}

			this.Gizmo = null;
			this.selection = value;
			this.selection?.OnSelected(true);

			if (this.selection != null && !this.selection.IsActive)
				this.selection.Activate();

			this.SelectionChanged?.Invoke(oldSelection, value);
			this.RaisePropertyChanged();

			this.DefaultGizmo();
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

	public SelectionGizmoBase? Gizmo
	{
		get => this.gizmo;
		set
		{
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
		}
	}

	public object? HoverSource { get; set; }

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
		this.selectionGizmos.Add(new RotationGizmo());
		this.selectionGizmos.Add(new TranslationGizmo());
		this.selectionGizmos.Add(new ScaleGizmo());

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
	}

	public override void Detach()
	{
		base.Detach();
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
	}

	public List<SelectionGizmoBase> GetValidGizmos()
	{
		List<SelectionGizmoBase> results = new();

		if (this.selection == null)
			return results;

		foreach (SelectionGizmoBase gizmo in this.selectionGizmos)
		{
			if (!gizmo.SupportsSelection(this.selection))
				continue;

			results.Add(gizmo);
		}

		return results;
	}

	protected void DefaultGizmo()
	{
		// TODO: Get the default gizmo from the selection actually.
		List<SelectionGizmoBase> results = this.GetValidGizmos();
		if (results.Count > 0)
		{
			this.Gizmo = results[0];
		}
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

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

namespace StudioFourteen.Services.Scene;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using StudioFourteen.Services.Rendering.Draw.Gizmos;

public interface ICreatableSceneObject ////: IDraggable
{
	Task Create();
}

public abstract partial class SceneObjectBase : ObservableObject, IDisposable
{
	private readonly List<GizmoBase> gizmos = new();

	public SceneObjectBase()
	{
		this.Name = string.Empty;

		this.Subtitle = this.GetType().Name;
	}

	public delegate void SceneObjectBaseBoolDelegate(SceneObjectBase sender, bool value);
	public event SceneObjectBaseBoolDelegate? IsSelectedChanged;
	public event SceneObjectBaseBoolDelegate? IsHoveredChanged;

	public abstract string Id { get; }

	[ObservableProperty] public partial string Name { get; set; }
	[ObservableProperty] public partial string? Subtitle { get; set; }
	[ObservableProperty] public partial string? Description { get; set; }
	[ObservableProperty] public partial bool IsReady { get; set; }
	[ObservableProperty] public partial bool IsHovered { get; set; }
	[ObservableProperty] public partial bool IsSelected { get; set; }
	[ObservableProperty] public partial int InspectorTab { get; set; }

	public override string ToString()
	{
		return $"{this.Id} ({base.ToString()})";
	}

	public void AddGizmo<T>()
		where T : SceneObjectGizmoBase
	{
		SceneObjectGizmoBase? gizmo = Activator.CreateInstance<T>();
		if (gizmo == null)
			return;

		this.AddGizmo(gizmo);
	}

	public void AddGizmo(SceneObjectGizmoBase gizmo)
	{
		this.gizmos.Add(gizmo);
		gizmo.SetTarget(this);
		gizmo.Enable();
	}

	public virtual void Dispose()
	{
		foreach (GizmoBase gizmo in this.gizmos)
		{
			gizmo.Disable();
		}
	}

	[RelayCommand]
	public virtual void Reset()
	{
	}

	[RelayCommand]
	public void InterfaceHover(bool value)
	{
		this.IsHovered = value;
	}

	[RelayCommand]
	public virtual void RemoveFromScene()
	{
		Studio.Scene.RemoveObject(this);
	}

	public virtual void OnSelected(bool value)
	{
		this.IsSelected = value;
	}

	public virtual void OnHovered(bool value)
	{
		this.IsHovered = value;
	}

	public virtual void OnGameTick()
	{
	}

	public bool Equals(SceneObjectBase? other)
	{
		return this.Id == other?.Id;
	}

	public virtual bool IsHit(HitInfo hitInfo) => false;

	partial void OnIsSelectedChanged(bool value)
	{
		if (value)
		{
			Studio.Scene.Select(this);
		}
		else
		{
			Studio.Scene.Deselect(this);
		}

		this.IsSelectedChanged?.Invoke(this, value);
	}

	partial void OnIsHoveredChanged(bool value)
	{
		this.IsHoveredChanged?.Invoke(this, value);
	}
}

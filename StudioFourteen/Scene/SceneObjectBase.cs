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

namespace StudioFourteen.Scene;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StudioFourteen.Controllers;
using StudioFourteen.DragAndDrop;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Utilities;
using WpfUtils.Commands;

public interface ICreatableSceneObject : IDraggable
{
	Task Create();
}

[NotifyPropertyChanged]
[Services]
[Logger]
public abstract partial class SceneObjectBase : IDisposable
{
	private SceneObjectControllerBase? controller;

	public SceneObjectBase()
	{
		this.Name = string.Empty;
		this.ResetCommand = new(this.Reset);
	}

	[Bind] public partial string Name { get; set; }
	[Bind] public partial string? Subtitle { get; set; }
	[Bind] public partial string? Description { get; set; }
	[Bind] public partial bool IsReady { get; set; }
	[Bind] public partial bool IsHovered { get; set; }
	[Bind] public partial bool IsSelected { get; set; }

	public abstract string Id { get; }
	public abstract object? Icon { get; }
	public abstract string TypeName { get; }

	public SimpleCommand ResetCommand { get; init; }
	public List<GizmoBase> Gizmos { get; init; } = new();

	public virtual void Dispose()
	{
		foreach (GizmoBase gizmo in this.Gizmos)
		{
			gizmo.Disable();
		}

		this.controller?.Dispose();
	}

	public virtual void Reset()
	{
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

	public T SetController<T>()
		where T : SceneObjectControllerBase
	{
		this.controller = Activator.CreateInstance(typeof(T), [this]) as SceneObjectControllerBase;

		if (this.controller == null || this.controller is not T tController)
			throw new Exception("Failed to set object controller");

		return tController;
	}
}

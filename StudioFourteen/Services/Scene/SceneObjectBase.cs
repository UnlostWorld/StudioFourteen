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
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public interface ICreatableSceneObject ////: IDraggable
{
	Task Create();
}

public abstract partial class SceneObjectBase : ObservableObject, IDisposable
{
	[ObservableProperty] private string name;
	[ObservableProperty] private string? subtitle;
	[ObservableProperty] private string? description;
	[ObservableProperty] private bool isReady;
	[ObservableProperty] private bool isHovered;
	[ObservableProperty] private bool isSelected;

	public SceneObjectBase()
	{
		this.Name = string.Empty;
	}

	public abstract string Id { get; }

	////public List<GizmoBase> Gizmos { get; init; } = new();

	public override string ToString()
	{
		return $"{this.Name} - {this.Id} ({base.ToString()})";
	}

	public virtual void Dispose()
	{
		/*foreach (GizmoBase gizmo in this.Gizmos)
		{
			gizmo.Disable();
		}*/
	}

	[RelayCommand]
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
}

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

namespace StudioFourteen.Rendering.Gizmos;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Settings;

public abstract class GizmoBase : SceneGroup, INotifyPropertyChanged
{
	public GizmoBase()
	{
		this.Persistence = Persistence.GetPersistence($"Gizmo_{this.GetType().Name}");

		this.Persistence.PersistenceChanged += this.OnPersistenceChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public abstract string Name { get; }

	public override bool IsVisible
	{
		get => this.GetPersistence<bool>(defaultValue: true);
		set => this.SetPersistence(value);
	}

	protected Persistence Persistence { get; init; }

	public virtual void Enable()
	{
		this.Services.Gizmos.Enable(this);
		this.OnPersistenceChanged();
	}

	public virtual void Disable()
	{
		this.Services.Gizmos.Disable(this);
	}

	public T? GetPersistence<T>([CallerMemberName] string id = "", T? defaultValue = default) => this.Persistence.GetPersistence<T>(id, defaultValue);

	public void SetPersistence(object? value, [CallerMemberName] string id = "")
	{
		this.Persistence.SetPersistence(value, id);
		this.PropertyChanged?.Invoke(this, new(id));
	}

	public void SetPersistence(string id, object? value)
	{
		this.Persistence.SetPersistence(id, value);
		this.PropertyChanged?.Invoke(this, new(id));
	}

	public virtual void OnGameTick()
	{
	}

	protected virtual void OnPersistenceChanged()
	{
	}

	private void OnPersistenceChanged(Persistence persistence)
	{
		this.OnPersistenceChanged();
	}

	/*
	// Test
	Vector2? mouse = this.Services.Input.Mouse?.GetPosition();
	if (mouse != null)
	{
		HitTestResult result = new();
		this.Geometry.HitTest(mouse.Value, ref result);

		this.Log.Information($">> {result.DrawObject} {result.Distance}");
	}
	*/
}
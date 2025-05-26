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

namespace StudioFourteen.Rendering.Scene.Gizmos;

using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;
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
	public virtual bool KeepScreenSize => true;

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

	protected virtual void OnPersistenceChanged()
	{
	}

	protected override void OnDraw()
	{
		if (this.KeepScreenSize)
		{
			this.LocalTransform = this.GetCameraScaleTransform();
		}

		base.OnDraw();
	}

	private void OnPersistenceChanged(Persistence persistence)
	{
		this.OnPersistenceChanged();
	}

	private Transform GetCameraScaleTransform()
	{
		Vector4 gizmoPos = Vector4.Transform(new Vector4(0, 0, 0, 1), this.Transform.ToMatrix());
		Vector4 vector = gizmoPos - new Vector4(this.Services.Camera.CurrentPosition, 1.0f);
		float distance = vector.Length() * 0.1f;
		return Transform.FromScale(distance);
	}
}
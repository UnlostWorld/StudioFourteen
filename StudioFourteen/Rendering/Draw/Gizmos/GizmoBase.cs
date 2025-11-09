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

namespace StudioFourteen.Rendering.Draw.Gizmos;

using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Passes;
using StudioFourteen.Settings;

public abstract class GizmoBase : DrawGroup, INotifyPropertyChanged
{
	protected ForwardPass? renderPass;

	public GizmoBase()
	{
		this.Persistence = Persistence.GetPersistence($"Gizmo_{this.GetType().Name}");

		this.Persistence.PersistenceChanged += this.OnPersistenceChanged;
	}

	public event PropertyChangedEventHandler? PropertyChanged;

	public abstract string Name { get; }
	public abstract bool KeepScreenSize { get; }
	public virtual bool ShowInControlPanel => true;

	public virtual bool IsVisibleInOverlay
	{
		get => this.GetPersistence(defaultValue: true);
		set => this.SetPersistence(value);
	}

	public virtual bool IsBeingManipulated
	{
		get => false;
	}

	protected Persistence Persistence { get; init; }

	public virtual void Enable(ForwardPass? pass = null)
	{
		if (pass == null)
			pass = RenderingService.OverlayRenderer.Forward;

		this.renderPass = pass;
		this.renderPass.Add(this);

		GizmoService.Enable(this);
		this.OnPersistenceChanged();
	}

	public virtual void Disable()
	{
		this.renderPass?.Remove(this);
		GizmoService.Disable(this);
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

	public override void Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		if (!this.IsVisibleInOverlay && renderer is GameOverlayRenderer)
			return;

		base.Draw(renderer, transform, device, deviceContext);

		if (this.KeepScreenSize && renderer is GameOverlayRenderer)
		{
			this.LocalTransform = this.GetCameraScaleTransform();
		}
	}

	protected virtual void OnPersistenceChanged()
	{
	}

	private void OnPersistenceChanged(Persistence persistence)
	{
		this.OnPersistenceChanged();
	}

	private Transform GetCameraScaleTransform()
	{
		Vector4 gizmoPos = Vector4.Transform(new Vector4(0, 0, 0, 1), this.Transform.ToMatrix());
		Vector4 vector = gizmoPos - new Vector4(this.CameraPosition, 1.0f);
		float scale = vector.Length() * 0.1f;
		return Transform.FromScale(scale);
	}
}
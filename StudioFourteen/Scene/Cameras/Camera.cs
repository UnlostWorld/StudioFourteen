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

namespace StudioFourteen.Scene.Cameras;

using PropertyChanged.SourceGenerator;
using StudioFourteen.Scene.Cameras.Modifiers;
using StudioFourteen.Scene;
using System;
using System.Collections.ObjectModel;

public abstract partial class Camera : SceneObjectBase, IDisposable
{
	// a distance of 0 hides the character, so a default of 3 seems good.
	private const float DefaultCameraDistance = 3;

	private readonly int cameraIndex = -1;
	private CameraState lastState;

	[Notify] private float fieldOfView;

	[Notify(Setter.Private)]
	private float groupPoseFovAdjust;

	public Camera()
	{
		this.cameraIndex = this.Services.Camera.RegisterCamera(this);

		this.Gizmos.Add(new CameraGizmo(this));
	}

	public override string Id => $"{this.GetType().Name}:{this.cameraIndex}";
	public override object? Icon => Resources.Find("ICON_Type_Character");

	public bool IsInitialized { get; set; } = false;

	public ObservableCollection<CameraModifierBase> Modifiers { get; init; } = new();
	public CameraState LastState => this.lastState;

	public bool IsActive
	{
		get => this.Services.Camera.Current == this;
		set => this.Services.Camera.Current = this;
	}

	public override void Dispose()
	{
		base.Dispose();

		this.Services.Camera.RemoveCamera(this);
	}

	public override void Reset()
	{
		base.Reset();
		this.IsInitialized = false;
	}

	public virtual void Initialize(CameraState currentState, Camera? previousCamera)
	{
		this.IsInitialized = true;

		this.FieldOfView = 75.0f;
	}

	public virtual void Tick(float deltaTime)
	{
		foreach(CameraModifierBase modifier in this.Modifiers)
		{
			modifier.Tick(deltaTime);
		}
	}

	public unsafe virtual void Calculate(ref CameraState state, Camera? blend = null, float blendWeight = 0)
	{
		state.FieldOfView = (this.FieldOfView + this.GroupPoseFovAdjust) / 100.0f;

		if (blend is OrbitCamera blendOrbit)
		{
			state.FieldOfView = float.Lerp(
				this.FieldOfView + this.GroupPoseFovAdjust,
				blendOrbit.FieldOfView + blendOrbit.GroupPoseFovAdjust,
				blendWeight) / 100.0f;
		}
	}

	public unsafe virtual void UpdateGroupPoseCamera(GroupPoseCamera* camera)
	{
		this.GroupPoseFovAdjust = camera->FoV * 100;
	}

	public virtual void Activate()
	{
	}

	public virtual void OnRender(ref CameraState state)
	{
		this.lastState = state;
	}

	public virtual void Deactivate()
	{
	}
}
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

using StudioFourteen.Scene.Cameras.Modifiers;
using StudioFourteen.Interop;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WpfUtils.Animation;

using CameraManager = FFXIVClientStructs.FFXIV.Client.Game.Control.CameraManager;
using GameCamera = FFXIVClientStructs.FFXIV.Client.Game.Camera;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;

public struct CameraState
{
	public Vector3 Position;
	public Quaternion Rotation;
	public float FieldOfView;
}

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public struct GroupPoseCamera
{
	[FieldOffset(0x0)]
	public GameCamera Camera;

	[FieldOffset(0x12C)] public float FoV;
	[FieldOffset(0x130)] public Vector2 Angle;
	[FieldOffset(0x150)] public Vector2 Pan;
	[FieldOffset(0x160)] public float Rotation;
	[FieldOffset(0x208)] public Vector2 Collide;
}

public class CameraService : ServiceBase
{
	private const float CameraBlendTimeMs = 1000;
	private readonly Stopwatch blendWatch = new();
	private readonly List<Camera> cameras = new();

	private Camera? current;
	private Camera? last;
	private CameraState state = default;
	private bool doAttachBlend = false;
	private int cameraIdCount = 0;

	public delegate void CamerasChangedDelegate();
	public delegate void CameraChangedDelegate(Camera? oldCamera, Camera? newCamera);

	public event CamerasChangedDelegate? CamerasChanged;
	public event CameraChangedDelegate? CurrentCameraChanged;

	public GroupPoseCamera? InitialCamera { get; private set; }

	public Camera? Current
	{
		get => this.current;
		set
		{
			if (value == this.current)
				return;

			this.last = this.current;
			this.current?.Deactivate();
			this.current = value;
			this.current?.Activate();
			this.blendWatch.Restart();

			this.RaisePropertyChanged();
			this.CurrentCameraChanged?.Invoke(this.last, this.current);
		}
	}

	public EasingFunctionBase BlendEase { get; set; } = new SineEase();

	public float NearPlane { get; private set; }
	public float FarPlane { get; private set; }
	public Matrix4x4 CurrentView { get; private set; }
	public Matrix4x4 CurrentProjection { get; private set; }
	public Matrix4x4 LastView { get; private set; }
	public Matrix4x4 LastProjection { get; private set; }
	public Vector3 CurrentPosition { get; private set; }
	public Vector3 CurrentForward { get; private set; }

	public override Task Start()
	{
		this.Services.GroupPose.StateChanged += this.OnGroupPoseStateChanged;
		this.OnGroupPoseStateChanged(this.Services.GroupPose.IsGroupPosing);

		return base.Start();
	}

	public unsafe override void Attach()
	{
		base.Attach();

		this.doAttachBlend = true;
		this.blendWatch.Restart();

		this.InitialCamera = *(GroupPoseCamera*)CameraManager.Instance()->Camera;

		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);

		Hooks.SceneCameraUpdate.Enable(this.SceneCameraUpdateDetour);
		Hooks.CameraMatrixLoad.Enable(this.CameraMatrixLoad);
		Hooks.GPoseCameraUpdate.Enable(this.GroupPoseCameraUpdateDetour);

		// Special case to reinitialize the orbit target camera each time
		// the camera service attaches so that any changes to the group pose camera
		// outside of studio gets kept.
		if (this.current is OrbitTargetCamera)
		{
			this.current.IsInitialized = false;
		}
	}

	public unsafe override void Detach()
	{
		base.Detach();

		GroupPoseCamera* camera = (GroupPoseCamera*)CameraManager.Instance()->Camera;

		// Restore camera settings
		if (camera != null && this.InitialCamera != null && this.Services.GroupPose.IsGroupPosing)
		{
			camera->FoV = this.InitialCamera.Value.FoV;
			camera->Pan = this.InitialCamera.Value.Pan;
			camera->Rotation = this.InitialCamera.Value.Rotation;
		}

		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);

		Hooks.SceneCameraUpdate.Disable();
		Hooks.CameraMatrixLoad.Disable();
		Hooks.GPoseCameraUpdate.Disable();

		this.blendWatch.Stop();
	}

	public Vector3 WorldToCamera(Vector3 worldPos)
	{
		Matrix4x4 viewProj = this.CurrentView * this.CurrentProjection;
		return viewProj.TransformViewProjection(worldPos);
	}

	public int RegisterCamera(Camera studioCameraBase)
	{
		lock (this.cameras)
		{
			this.cameras.Add(studioCameraBase);
			this.cameraIdCount++;
			return this.cameraIdCount;
		}
	}

	public void RemoveCamera(Camera studioCameraBase)
	{
		lock (this.cameras)
		{
			this.cameras.Remove(studioCameraBase);
		}
	}

	protected void OnGameTick()
	{
		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
			this.current.OnGameTick();
		}
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		if (newState && this.cameras.Count <= 0)
		{
			this.Current = this.Services.Scene.AddObject<OrbitTargetCamera>("Editor Camera");
		}
		else if (newState && this.cameras.Count > 0)
		{
			this.Current = this.cameras[0];
		}
		else
		{
			this.Current = null;
		}
	}

	private unsafe nint GroupPoseCameraUpdateDetour(GroupPoseCamera* camera)
	{
		if (this.Services.GroupPose.IsGroupPosing
			&& !this.doAttachBlend
			&& this.current != null)
		{
			this.current?.UpdateGroupPoseCamera(camera);
		}

		return Hooks.GPoseCameraUpdate.Original(camera);
	}

	private unsafe nint SceneCameraUpdateDetour(SceneCamera* camera)
	{
		nint result = Hooks.SceneCameraUpdate.Original(camera);

		float deltaTime = 60 / 1000.0f;

		this.NearPlane = camera->RenderCamera->NearPlane;
		this.FarPlane = camera->RenderCamera->FarPlane;

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
			try
			{
				if (!this.current.IsInitialized)
				{
					this.current.Initialize(this.state, this.last);
				}

				float blendValue = 1;
				if (this.last != null || this.doAttachBlend)
				{
					blendValue = this.blendWatch.ElapsedMilliseconds / CameraBlendTimeMs;
					blendValue = this.BlendEase.Ease(blendValue, EasingFunctionBase.EasingModes.EaseInOut);

					if (this.blendWatch.ElapsedMilliseconds > CameraBlendTimeMs)
					{
						blendValue = 1;
						this.last = null;
						this.doAttachBlend = false;
						this.blendWatch.Stop();
					}
				}

				this.state.FieldOfView = camera->RenderCamera->FoV;

				if (!this.Services.Photos.IsCapturing)
					this.current.Tick(deltaTime);

				this.current.Calculate(ref this.state, this.last, 1 - blendValue);

				foreach (CameraModifierBase modifier in this.current.Modifiers)
				{
					modifier.Calculate(ref this.state, blendValue);
				}

				this.current.OnRender(ref this.state);

				Vector3 forward = Vector3.Transform(new(1, 0, 0), this.state.Rotation);
				Vector3 up = Vector3.Transform(new(0, 1, 0), this.state.Rotation);

				this.CurrentPosition = this.state.Position;
				this.CurrentForward = forward;

				Matrix4x4 newMatrix = Matrix4x4.CreateLookTo(this.state.Position, forward, up);

				if (this.doAttachBlend && this.InitialCamera != null)
				{
					Matrix4x4 initialMatrix = this.InitialCamera.Value.Camera.SceneCamera.ViewMatrix;
					camera->ViewMatrix = Matrix4x4.Lerp(camera->ViewMatrix, newMatrix, blendValue);
				}
				else
				{
					camera->ViewMatrix = newMatrix;
				}

				this.CameraMatrixLoad(camera->RenderCamera, (nint)(&camera->ViewMatrix));

				// This does update the Fov, but the projection matrix we read does not inherit it
				// for some reason, so lets just use the gpose FoV values.
				////camera->RenderCamera->FoV = this.state.FieldOfView;

				// Update all cameras in the background.
				// TODO: we could move this to another thread to ensure
				// the camera detour is fast.
				lock (this.cameras)
				{
					CameraState temp = default;
					foreach (Camera otherCamera in this.cameras)
					{
						if (otherCamera == this.current)
							continue;

						if (!this.Services.Photos.IsCapturing)
							otherCamera.Tick(deltaTime);

						otherCamera.Calculate(ref temp);
						otherCamera.OnRender(ref temp);
					}
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error in camera update");
			}
		}
		else
		{
			// For some reason depth doesn't work unless we set the View Matrix to a LookTo
			// that we create, even though we use the same view matrix in the renderer service.
			this.CurrentPosition = camera->Position;
			this.CurrentForward = Vector3.Transform(Vector3.UnitX, camera->Rotation);
			Vector3 up = Vector3.Transform(new(0, 1, 0), camera->Rotation);
			camera->ViewMatrix = Matrix4x4.CreateLookTo(this.CurrentPosition, this.CurrentForward, up);
		}

		this.LastView = this.CurrentView;
		this.LastProjection = this.CurrentProjection;

		this.CurrentView = camera->RenderCamera->ViewMatrix;
		this.CurrentProjection = camera->RenderCamera->ProjectionMatrix;

		return result;
	}

	private unsafe void CameraMatrixLoad(RenderCamera* camera, nint a1)
	{
		if (!Hooks.CameraMatrixLoad.IsValid)
			return;

		Hooks.CameraMatrixLoad.Original(camera, a1);
	}
}
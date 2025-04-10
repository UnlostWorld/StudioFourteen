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

namespace StudioFourteen.Cameras;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using StudioFourteen.Cameras.Modifiers;
using StudioFourteen.Interop;
using StudioFourteen.Plugin;
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

	private StudioCameraBase? current;
	private StudioCameraBase? last;
	private CameraState state = default;
	private bool doAttachBlend = false;

	public delegate void CamerasChangedDelegate();
	public delegate void CameraChangedDelegate(StudioCameraBase? oldCamera, StudioCameraBase? newCamera);

	public event CamerasChangedDelegate? CamerasChanged;
	public event CameraChangedDelegate? CurrentCameraChanged;

	public GroupPoseCamera? InitialCamera { get; private set; }

	public StudioCameraBase? Current
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

	public List<StudioCameraBase> Cameras { get; init; } = new();

	public Matrix4x4 CurrentView { get; private set; }
	public Matrix4x4 CurrentProjection { get; private set; }
	public Matrix4x4 CurrentViewProjection { get; private set; }

	public override Task Start()
	{
		this.Cameras.Add(new OrbitTargetCamera());

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

	public void CreateCamera<T>(bool activate = true)
		where T : StudioCameraBase
	{
		StudioCameraBase? cam = Activator.CreateInstance<T>();
		this.Cameras.Add(cam);
		this.CamerasChanged?.Invoke();

		this.Current = cam;
	}

	public void DeleteCamera(StudioCameraBase camera)
	{
		bool wasCurrent = this.current == camera;

		this.Cameras.Remove(camera);

		if (this.Cameras.Count <= 0)
			this.Cameras.Add(new OrbitTargetCamera());

		this.CamerasChanged?.Invoke();

		if (wasCurrent)
		{
			this.Current = this.Cameras[0];
		}

		camera.Dispose();
	}

	public void LoadCameras(IEnumerable<StudioCameraBase> cameras)
	{
		List<StudioCameraBase> oldCameras = new(this.Cameras);

		this.Cameras.Clear();
		foreach (StudioCameraBase newCamera in cameras)
		{
			newCamera.IsInitialized = true;
			this.Cameras.Add(newCamera);
		}

		this.CamerasChanged?.Invoke();
		this.Current = this.Cameras[0];

		foreach (StudioCameraBase camera in oldCameras)
		{
			camera.Dispose();
		}
	}

	public Vector3 WorldToCamera(Vector3 worldPos)
	{
		return this.CurrentViewProjection.TransformViewProjection(worldPos);
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
		if (newState)
		{
			this.Current = this.Cameras[0];
			this.state.Position = this.Services.GroupPose.EnterPosition + new Vector3(0, 1.5f, 0);
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

				foreach(CameraModifierBase modifier in this.current.Modifiers)
				{
					modifier.Calculate(ref this.state, blendValue);
				}

				this.current.OnRender(ref this.state);

				// in portrait preview mode, rotate the camera 90 degrees.
				if (this.Services.Photos.IsPortrait)
					this.state.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -1.5707964f);

				Vector3 forward = Vector3.Transform(new(1, 0, 0), this.state.Rotation);
				Vector3 up = Vector3.Transform(new(0, 1, 0), this.state.Rotation);

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

				camera->RenderCamera->FoV = this.state.FieldOfView;

				// Update all cameras in the background.
				// TODO: we could move this to another thread to ensure
				// the camera detour is fast.
				CameraState temp = default;
				foreach (StudioCameraBase otherCamera in this.Cameras)
				{
					if (otherCamera == this.current)
						continue;

					if (!this.Services.Photos.IsCapturing)
						otherCamera.Tick(deltaTime);

					otherCamera.Calculate(ref temp);
					otherCamera.OnRender(ref temp);
				}
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error in camera update");
			}
		}

		this.CurrentView = camera->ViewMatrix;
		this.CurrentProjection = camera->RenderCamera->ProjectionMatrix;
		this.CurrentViewProjection = this.CurrentView * this.CurrentProjection;

		return result;
	}

	private unsafe void CameraMatrixLoad(RenderCamera* camera, nint a1)
	{
		Hooks.CameraMatrixLoad.Original(camera, a1);
	}
}
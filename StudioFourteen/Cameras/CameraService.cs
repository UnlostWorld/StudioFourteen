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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Kernel;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;
using StudioFourteen.Services;
using StudioFourteen.Utilities;
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

	private Hook<GPoseCameraUpdateDelegate>? gPoseCameraUpdateHook;
	private Hook<SceneCameraUpdateDelegate>? sceneCameraUpdateHook;
	private Hook<CameraMatrixLoadDelegate>? cameraMatrixLoadHook;

	private StudioCameraBase? current;
	private StudioCameraBase? last;
	private CameraState state = default;
	private bool doAttachBlend = false;
	private Matrix4x4 lastViewMatrix;
	private Matrix4x4 lastProjectionMatrix;

	public delegate void CamerasChangedDelegate();
	public delegate void CameraChangedDelegate(StudioCameraBase? oldCamera, StudioCameraBase? newCamera);
	private unsafe delegate nint GPoseCameraUpdateDelegate(GroupPoseCamera* camera);
	private unsafe delegate nint SceneCameraUpdateDelegate(SceneCamera* sceneCamera);
	private unsafe delegate void CameraMatrixLoadDelegate(RenderCamera* camera, nint a1);

	public event CamerasChangedDelegate? CamerasChanged;
	public event CameraChangedDelegate? CurrentCameraChanged;

	public GroupPoseCamera? InitialCamera { get; private set; }

	public StudioCameraBase? Current
	{
		get => this.current;
		set
		{
			if (value == null)
				return;

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

		this.sceneCameraUpdateHook = InteropService.HookFromSignature<SceneCameraUpdateDelegate>("48 ?? ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 EC ?? ?? ?? ?? 48 8B ?? 48 ?? ?? ??", this.SceneCameraUpdateDetour);
		this.sceneCameraUpdateHook?.Enable();

		this.cameraMatrixLoadHook = InteropService.HookFromSignature<CameraMatrixLoadDelegate>("E8 ?? ?? ?? ?? 48 8B 93 90 02 ?? ?? 48 8D 4C 24 40", this.CameraMatrixLoad);
		this.cameraMatrixLoadHook?.Enable();

		this.gPoseCameraUpdateHook = InteropService.HookFromSignature<GPoseCameraUpdateDelegate>("40 55 53 57 48 8D 6C 24 A0 48 81 EC ?? ?? ?? ?? 48 8B 1D", this.GroupPoseCameraUpdateDetour);
		this.gPoseCameraUpdateHook?.Enable();

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

		this.sceneCameraUpdateHook?.Dispose();
		this.cameraMatrixLoadHook?.Dispose();
		this.gPoseCameraUpdateHook?.Dispose();

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

	public unsafe bool WorldToCamera(Vector3 worldPos, out Vector3 screenPos)
	{
		screenPos = Vector3.Zero;

		Vector4 vector = Vector4.Transform(new Vector4(worldPos, 1f), this.lastViewMatrix * this.lastProjectionMatrix);
		if (vector.W < float.Epsilon)
			return false;

		float d = vector.Z;

		vector *= MathF.Abs(1f / vector.W);
		screenPos = new Vector3
		{
			X = (vector.X + 1f) * 0.5f,
			Y = (1f - vector.Y) * 0.5f,
			Z = d,
		};

		if (screenPos.X < 0 || screenPos.X > 1)
			return false;

		if (screenPos.Y < 0 || screenPos.Y > 1)
			return false;

		return true;
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
			this.current.OnFrameworkUpdate(framework);
		}
	}

	private void OnGroupPoseStateChanged(bool newState)
	{
		if (newState)
		{
			this.Current = this.Cameras[0];
		}
		else
		{
			this.Current = null;
		}
	}

	private unsafe nint GroupPoseCameraUpdateDetour(GroupPoseCamera* camera)
	{
		if (this.gPoseCameraUpdateHook == null)
			return 0;

		if (this.Services.GroupPose.IsGroupPosing
			&& !this.doAttachBlend
			&& this.current != null)
		{
			this.current?.UpdateGroupPoseCamera(camera);
		}

		return this.gPoseCameraUpdateHook.Original(camera);
	}

	private unsafe nint SceneCameraUpdateDetour(SceneCamera* camera)
	{
		if (this.sceneCameraUpdateHook == null)
			return 0;

		nint result = this.sceneCameraUpdateHook.Original(camera);

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
			if (!this.current.IsInitialized)
			{
				this.current.Initialize(this.state);
			}

			float blendValue = 0;
			if (this.last != null || this.doAttachBlend)
			{
				blendValue = this.blendWatch.ElapsedMilliseconds / CameraBlendTimeMs;
				blendValue = this.BlendEase.Ease(blendValue, EasingFunctionBase.EasingModes.EaseInOut);

				if (this.blendWatch.ElapsedMilliseconds > CameraBlendTimeMs)
				{
					blendValue = 0;
					this.last = null;
					this.doAttachBlend = false;
					this.blendWatch.Stop();
				}
			}

			this.state.FieldOfView = camera->RenderCamera->FoV;

			this.current.Tick(FramerateService.AverageDeltaTime);
			this.current.Calculate(ref this.state, this.last, 1 - blendValue);
			this.current.OnRender(ref this.state);

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

				otherCamera.Tick(FramerateService.AverageDeltaTime);
				otherCamera.Calculate(ref temp);
				otherCamera.OnRender(ref temp);
			}
		}

		this.lastViewMatrix = camera->ViewMatrix;
		this.lastProjectionMatrix = camera->RenderCamera->ProjectionMatrix;

		return result;
	}

	private unsafe void CameraMatrixLoad(RenderCamera* camera, nint a1)
	{
		if (this.cameraMatrixLoadHook == null || this.cameraMatrixLoadHook.IsDisposed)
			return;

		this.cameraMatrixLoadHook.Original(camera, a1);
	}
}
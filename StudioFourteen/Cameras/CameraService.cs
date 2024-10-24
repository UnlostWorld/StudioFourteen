// Brio
// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs

namespace StudioFourteen.Cameras;

using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using WpfUtils.Animation;

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

	private unsafe delegate nint GPoseCameraUpdateDelegate(GroupPoseCamera* camera);
	private unsafe delegate nint SceneCameraUpdateDelegate(SceneCamera* sceneCamera);
	private unsafe delegate void CameraMatrixLoadDelegate(RenderCamera* camera, nint a1);

	public StudioCameraBase? Current
	{
		get => this.current;
		set
		{
			this.last = this.current;
			this.current?.Deactivate();
			this.current = value;
			this.current?.Activate();
			this.blendWatch.Restart();

			this.RaisePropertyChanged();
		}
	}

	public EasingFunctionBase BlendEase { get; set; } = new SineEase();

	public ObservableCollection<StudioCameraBase> Cameras { get; init; } = new();

	public override Task Start()
	{
		this.current = new OrbitTargetCamera();
		this.Cameras.Add(this.current);

		unsafe
		{
			this.sceneCameraUpdateHook = InteropService.HookFromSignature<SceneCameraUpdateDelegate>("48 ?? ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 EC ?? ?? ?? ?? 48 8B ?? 48 ?? ?? ??", this.SceneCameraUpdateDetour);
			this.sceneCameraUpdateHook?.Enable();

			this.cameraMatrixLoadHook = InteropService.HookFromSignature<CameraMatrixLoadDelegate>("E8 ?? ?? ?? ?? 48 8B 93 90 02 ?? ?? 48 8D 4C 24 40", this.CameraMatrixLoad);
			this.cameraMatrixLoadHook?.Enable();

			this.gPoseCameraUpdateHook = InteropService.HookFromSignature<GPoseCameraUpdateDelegate>("40 55 53 57 48 8D 6C 24 A0 48 81 EC ?? ?? ?? ?? 48 8B 1D", this.GroupPoseCameraUpdateDetour);
			this.gPoseCameraUpdateHook?.Enable();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		this.sceneCameraUpdateHook?.Disable();
		this.cameraMatrixLoadHook?.Disable();

		return base.Stop();
	}

	public void CreateCamera<T>(bool activate = true)
		where T : StudioCameraBase
	{
		StudioCameraBase? cam = Activator.CreateInstance<T>();
		this.Cameras.Add(cam);
		this.Current = cam;
	}

	protected override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
			float deltaTime = (float)framework.UpdateDelta.TotalMilliseconds / 1000.0f;

			// Update delta seems to be quite unstable. unsure why.
			deltaTime = 0.02f;

			this.current.Update(deltaTime);
		}
	}

	private unsafe nint GroupPoseCameraUpdateDetour(GroupPoseCamera* camera)
	{
		if (this.gPoseCameraUpdateHook == null)
			return 0;

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
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
			if (this.last != null)
			{
				blendValue = this.blendWatch.ElapsedMilliseconds / CameraBlendTimeMs;

				blendValue = this.BlendEase.Ease(blendValue, EasingFunctionBase.EasingModes.EaseInOut);

				if (this.blendWatch.ElapsedMilliseconds > CameraBlendTimeMs)
				{
					blendValue = 0;
					this.last = null;
				}
			}

			this.state.FieldOfView = camera->RenderCamera->FoV;

			this.current.Calculate(ref this.state, this.last, 1 - blendValue);

			Vector3 forward = Vector3.Transform(new(1, 0, 0), this.state.Rotation);
			Vector3 up = Vector3.Transform(new(0, 1, 0), this.state.Rotation);
			camera->ViewMatrix = Matrix4x4.CreateLookTo(this.state.Position, forward, up);

			this.CameraMatrixLoad(camera->RenderCamera, (nint)(&camera->ViewMatrix));

			camera->RenderCamera->FoV = this.state.FieldOfView;
		}

		return result;
	}

	private unsafe void CameraMatrixLoad(RenderCamera* camera, nint a1)
	{
		if (this.cameraMatrixLoadHook == null)
			return;

		this.cameraMatrixLoadHook.Original(camera, a1);
	}
}
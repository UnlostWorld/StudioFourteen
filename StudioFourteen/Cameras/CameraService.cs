// Brio
// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs

namespace StudioFourteen.Cameras;

using Dalamud.Hooking;
using StudioFourteen.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
internal struct Camera
{
	[FieldOffset(0x12C)] public float FoV;
	[FieldOffset(0x130)] public Vector2 Angle;
	[FieldOffset(0x150)] public Vector2 Pan;
	[FieldOffset(0x160)] public float Rotation;
	[FieldOffset(0x208)] public Vector2 Collide;

	[FieldOffset(16)] public SceneCamera SceneCamera;
}

public class CameraService : ServiceBase
{
	private const float CameraBlendTimeMs = 1000;
	private readonly Stopwatch blendWatch = new();

	private Hook<SceneCameraUpdateDelegate>? cameraUpdateHook;
	private Hook<CameraMatrixLoadDelegate>? cameraMatrixLoadHook;

	private StudioCameraBase? current;
	private StudioCameraBase? last;

	private unsafe delegate nint SceneCameraUpdateDelegate(SceneCamera* sceneCamera);
	private unsafe delegate void CameraMatrixLoadDelegate(RenderCamera* camera, nint a1);

	public StudioCameraBase? Current
	{
		get => this.current;
		set
		{
			this.last = this.current;
			this.current = value;
			this.blendWatch.Restart();

			this.RaisePropertyChanged();
		}
	}

	public List<StudioCameraBase> Cameras { get; init; } = new();

	public override Task Start()
	{
		this.current = new OrbitTargetCamera();
		this.Cameras.Add(this.current);

		unsafe
		{
			this.cameraUpdateHook = InteropService.HookFromSignature<SceneCameraUpdateDelegate>("48 ?? ?? ?? ?? ?? 48 81 EC ?? ?? ?? ?? F6 81 EC ?? ?? ?? ?? 48 8B ?? 48 ?? ?? ??", this.CameraUpdateDetour);
			this.cameraUpdateHook?.Enable();

			this.cameraMatrixLoadHook = InteropService.HookFromSignature<CameraMatrixLoadDelegate>("E8 ?? ?? ?? ?? 48 8B 93 90 02 ?? ?? 48 8D 4C 24 40", this.CameraMatrixLoad);
			this.cameraMatrixLoadHook?.Enable();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		this.cameraUpdateHook?.Disable();
		this.cameraMatrixLoadHook?.Disable();

		return base.Stop();
	}

	private unsafe nint CameraUpdateDetour(SceneCamera* camera)
	{
		if (this.cameraUpdateHook == null)
			return 0;

		nint result = this.cameraUpdateHook.Original(camera);

		if (this.Services.GroupPose.IsGroupPosing)
		{
			this.Write(camera);
		}

		return result;
	}

	private unsafe void CameraMatrixLoad(RenderCamera* camera, nint a1)
	{
		if (this.cameraMatrixLoadHook == null)
			return;

		this.cameraMatrixLoadHook.Original(camera, a1);
	}

	private unsafe void Write(SceneCamera* camera)
	{
		if (this.current == null)
			return;

		/*if (this.last != null)
		{
			float value = this.blendWatch.ElapsedMilliseconds / CameraBlendTimeMs;

			if (value > 1.0)
			{
				this.last = null;
			}
		}
		else
		{
			this.live.Import(this.current);
		}*/

		camera->ViewMatrix = this.current.Calculate();
		this.CameraMatrixLoad(camera->RenderCamera, (nint)(&camera->ViewMatrix));
	}
}
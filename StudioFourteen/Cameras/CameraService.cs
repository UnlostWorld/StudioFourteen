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
using WpfUtils.Animation;
using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;

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

	public EasingFunctionBase BlendEase { get; set; } = new SineEase();

	public List<StudioCameraBase> Cameras { get; init; } = new();

	public override Task Start()
	{
		this.current = new OrbitTargetCamera();
		this.Cameras.Add(this.current);

		this.Cameras.Add(new OrbitTargetCamera());

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

		if (this.Services.GroupPose.IsGroupPosing && this.current != null)
		{
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

			camera->ViewMatrix = this.current.Calculate(this.last, 1 - blendValue);
			this.CameraMatrixLoad(camera->RenderCamera, (nint)(&camera->ViewMatrix));
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
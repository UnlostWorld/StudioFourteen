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

namespace StudioFourteen.Services.Rendering;

using System.Numerics;
using StudioFourteen.Services.Interop;
using StudioFourteen.Services.Interop.Structs;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Tick;

using RenderCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Camera;
using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;

public class CameraService : IService
{
	public unsafe CameraService()
	{
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);

		Hooks.SceneCameraUpdate.Enable(this.SceneCameraUpdateDetour);
		Hooks.CameraMatrixLoad.Enable(this.CameraMatrixLoad);
		Hooks.CameraUpdate.Enable(this.CameraUpdateDetour);
	}

	public float NearPlane { get; private set; }
	public float FarPlane { get; private set; }
	public Matrix4x4 CurrentView { get; private set; }
	public Matrix4x4 CurrentProjection { get; private set; }
	public Matrix4x4 LastView { get; private set; }
	public Matrix4x4 LastProjection { get; private set; }
	public Vector3 CurrentPosition { get; private set; }
	public Vector3 CurrentForward { get; private set; }

	public void Dispose()
	{
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);

		Hooks.SceneCameraUpdate.Disable();
		Hooks.CameraMatrixLoad.Disable();
		Hooks.CameraUpdate.Disable();
	}

	public Vector3 WorldToCamera(Vector3 worldPos)
	{
		Matrix4x4 viewProj = this.CurrentView * this.CurrentProjection;
		return viewProj.TransformViewProjection(worldPos);
	}

	protected void OnGameTick()
	{
	}

	private unsafe nint CameraUpdateDetour(GameCamera* camera)
	{
		return Hooks.CameraUpdate.Original(camera);
	}

	private unsafe nint SceneCameraUpdateDetour(SceneCamera* camera)
	{
		nint result = Hooks.SceneCameraUpdate.Original(camera);

		this.NearPlane = camera->RenderCamera->NearPlane;
		this.FarPlane = camera->RenderCamera->FarPlane;

		this.CurrentPosition = camera->Position;
		Vector3 forward = (Vector3)camera->LookAtVector - this.CurrentPosition;
		this.CurrentForward = Vector3.Normalize(forward);
		this.LastView = this.CurrentView;
		this.LastProjection = this.CurrentProjection;

		var view = camera->ViewMatrix;
		view.M44 = 1;
		this.CurrentView = view;
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
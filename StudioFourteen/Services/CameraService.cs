// Brio
// https://github.com/Etheirys/Brio/blob/main/Brio/Game/Camera/CameraService.cs

namespace StudioFourteen.Services;

using Dalamud.Hooking;
using StudioFourteen.Mvm;
using StudioFourteen.Structs.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using SceneCamera = FFXIVClientStructs.FFXIV.Client.Graphics.Scene.Camera;

[StructLayout(LayoutKind.Explicit, Size = 0x2B0)]
public struct Camera
{
	[FieldOffset(0x10)] public SceneCamera SceneCamera;
	[FieldOffset(0x100)] public uint UnkUInt;
	[FieldOffset(0x108)] public uint UnkFlags;

	[FieldOffset(0x114)] public float Distance;
	[FieldOffset(0x118)] public float MinDistance;
	[FieldOffset(0x11C)] public float MaxDistance;
	[FieldOffset(0x120)] public float FoV;
	[FieldOffset(0x124)] public float MinFoV;
	[FieldOffset(0x128)] public float MaxFoV;
	[FieldOffset(0x12C)] public float GroupPoseFovAdjust;
	[FieldOffset(0x130)] public Vector2 Angle;
	[FieldOffset(0x150)] public Vector2 Pan;
	[FieldOffset(0x160)] public float Rotation;
	[FieldOffset(0x17C)] public float InterpDistance;
	[FieldOffset(0x188)] public float SavedDistance;
	[FieldOffset(0x208)] public Vector2 Collide;
}

public class StudioCamera : ViewModel
{
	private nint cameraAddress;
	private Camera lastCamera;

	public int Index { get; set; }
	[AutoNotify] public string Name { get; set; } = "Default";

	[AutoNotify] public bool DisableCollisions { get; set; } = false;

	[AutoNotify] public Vector3 Position { get; set; }
	[AutoNotify] public Vector3 LookAt { get; set; }
	[AutoNotify] public float Distance { get; set; }
	[AutoNotify] public float FoV { get; private set; }
	[AutoNotify] public float GroupPoseFovAdjust { get; set; }
	[AutoNotify] public Quaternion Rotation { get; set; }

	[AutoNotify]
	public float GroupPoseFov
	{
		get => (this.FoV + this.GroupPoseFovAdjust) * 100.0f;
		set => this.GroupPoseFovAdjust = (value / 100.0f) - this.FoV;
	}

	public unsafe bool IsCamera(Camera* camera)
	{
		if (this.cameraAddress == 0)
			return true;

		return (nint)camera == this.cameraAddress;
	}

	public unsafe void Update(Camera* camera)
	{
		this.cameraAddress = (nint)camera;

		this.Position = this.UpdateValue(ref camera->SceneCamera.Object.Position, this.lastCamera.SceneCamera.Object.Position, this.Position);

		this.LookAt = camera->SceneCamera.LookAtVector;
		this.Distance = camera->Distance;

		this.FoV = camera->FoV;
		this.GroupPoseFovAdjust = this.UpdateValue(ref camera->GroupPoseFovAdjust, this.lastCamera.GroupPoseFovAdjust, this.GroupPoseFovAdjust);

		Vector3 euler = default;
		euler.Y = (camera->Angle.X * QuaternionExtensions.Rad2Deg) - 180;
		euler.Z = -(camera->Angle.Y * QuaternionExtensions.Rad2Deg);
		euler.X = camera->Rotation * QuaternionExtensions.Rad2Deg;

		Quaternion q = this.Rotation;
		q.FromEuler(euler);
		this.Rotation = q;

		this.lastCamera = *camera;
	}

	public unsafe void Collide(Camera* camera)
	{
		if (this.DisableCollisions)
		{
			camera->Collide = new Vector2(camera->MaxDistance);
		}
	}

	private T UpdateValue<T>(ref T live, T cacheValue, T viewModelValue)
		where T : IEquatable<T>
	{
		if (viewModelValue.Equals(cacheValue))
		{
			return live;
		}
		else
		{
			live = viewModelValue;
			return viewModelValue;
		}
	}
}

public class CameraService : ServiceBase
{
	private int currentCameraIndex = 0;

	private Hook<CameraCollisionDelegate>? cameraCollisionHook;
	private Hook<CameraUpdateDelegate>? cameraUpdateHook;

	private unsafe delegate nint CameraCollisionDelegate(Camera* a1, Vector3* a2, Vector3* a3, float a4, nint a5, float a6);
	private unsafe delegate nint CameraUpdateDelegate(Camera* camera);

	public StudioCamera Current
	{
		get => this.Cameras[this.currentCameraIndex];
		set => this.currentCameraIndex = this.Cameras.IndexOf(value);
	}

	public List<StudioCamera> Cameras { get; init; } = new()
	{
		new(),
	};

	public override Task Start()
	{
		unsafe
		{
			this.cameraCollisionHook = InteropService.HookFromSignature<CameraCollisionDelegate>("E8 ?? ?? ?? ?? 4C 8D 45 ?? 89 83", this.CameraCollisionDetour);
			this.cameraCollisionHook?.Enable();

			// Camera.vf3
			this.cameraUpdateHook = InteropService.HookFromSignature<CameraUpdateDelegate>("40 55 53 57 48 8D 6C 24 A0 48 81 EC ?? ?? ?? ?? 48 8B 1D", this.CameraUpdateDetour);
			this.cameraUpdateHook?.Enable();
		}

		return base.Start();
	}

	public override Task Stop()
	{
		this.cameraCollisionHook?.Disable();

		return base.Stop();
	}

	private unsafe nint CameraCollisionDetour(Camera* camera, Vector3* a2, Vector3* a3, float a4, nint a5, float a6)
	{
		if (this.cameraCollisionHook == null)
			return 0;

		if (this.Services.GroupPose.IsGroupPosing && this.Current.IsCamera(camera))
		{
			this.Current.Collide(camera);
		}

		return this.cameraCollisionHook.Original(camera, a2, a3, a4, a5, a6);
	}

	private unsafe nint CameraUpdateDetour(Camera* camera)
	{
		if (this.cameraUpdateHook == null)
			return 0;

		nint result = this.cameraUpdateHook.Original(camera);

		if (this.Services.GroupPose.IsGroupPosing && this.Current.IsCamera(camera))
		{
			this.Current.Index = this.currentCameraIndex;
			this.Current.Update(camera);
		}

		return result;
	}
}
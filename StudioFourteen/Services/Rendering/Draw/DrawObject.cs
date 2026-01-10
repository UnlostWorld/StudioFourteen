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

namespace StudioFourteen.Services.Rendering.Draw;

using System;
using System.Numerics;
using Serilog;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Numerics;

public abstract partial class DrawObject : IDisposable
{
	public Transform Transform = Transform.Identity;
	public DrawObject? Parent;

	public virtual bool IsHitTestVisible { get; set; } = true;
	public virtual bool IsVisible { get; set; } = true;

	public Vector3 WorldPosition { get; private set; }
	public Quaternion WorldRotation { get; private set; }
	public Vector3 WorldScale { get; private set; }

	public Vector3 CameraPosition { get; set; }

	protected Transform WorldTransform { get; private set; }
	protected Transform LocalTransform { get; set; } = Transform.Identity;

	protected Renderer? CurrentRenderer { get; private set; }

	public virtual void Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		this.CameraPosition = renderer.GetCameraPosition(renderer);
		this.WorldTransform = this.LocalTransform * this.Transform * transform;

		if (Matrix4x4.Decompose(this.WorldTransform.ToMatrix(), out Vector3 scale, out Quaternion rotation, out Vector3 translation))
		{
			this.WorldPosition = translation;
			this.WorldRotation = rotation;
			this.WorldScale = scale;
		}

		this.CurrentRenderer = renderer;
		this.OnDraw();
	}

	public abstract void Dispose();

	public abstract void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		HitTestResult result);

	public virtual T? GetParent<T>()
	{
		if (this is T tThis)
			return tThis;

		if (this.Parent != null)
			return this.Parent.GetParent<T>();

		return default;
	}

	protected virtual void OnDraw()
	{
	}
}

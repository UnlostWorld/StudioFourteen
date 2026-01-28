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

namespace StudioFourteen.Services.Rendering.Draw.Gizmos;

using System.ComponentModel;
using System.Numerics;
using System.Runtime.CompilerServices;
using SharpDX.Direct3D11;
using StudioFourteen.Services.Numerics;
using StudioFourteen.Services.Rendering.Passes;
using StudioFourteen.Services.Tick;

public abstract class GizmoBase : DrawGroup
{
	protected ForwardPass? renderPass;

	public abstract bool KeepScreenSize { get; }

	public virtual bool IsBeingManipulated
	{
		get => false;
	}

	public virtual void Enable(ForwardPass? pass = null)
	{
		if (pass == null)
			pass = Studio.Rendering.OverlayRenderer.Forward;

		this.renderPass = pass;
		this.renderPass.Add(this);
		Studio.Tick.Add(TickChannels.Game, this.OnGameTick);
		this.IsVisible = true;
	}

	public virtual void Disable()
	{
		this.IsVisible = false;
		this.renderPass?.Remove(this);
		Studio.Tick.Remove(TickChannels.Game, this.OnGameTick);
	}

	public virtual void OnGameTick()
	{
	}

	public override bool Draw(Renderer renderer, Transform transform, Device device, DeviceContext deviceContext)
	{
		////if (!this.IsVisibleInOverlay && renderer is GameOverlayRenderer)
		////	return false;

		if (!base.Draw(renderer, transform, device, deviceContext))
			return false;

		if (this.KeepScreenSize && renderer is GameOverlayRenderer)
		{
			this.LocalTransform = this.GetCameraScaleTransform();
		}

		return true;
	}

	private Transform GetCameraScaleTransform()
	{
		Vector4 gizmoPos = Vector4.Transform(new Vector4(0, 0, 0, 1), this.Transform.ToMatrix());
		Vector4 vector = gizmoPos - new Vector4(this.CameraPosition, 1.0f);
		float scale = vector.Length() * 0.1f;
		return Transform.FromScale(scale);
	}
}
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

namespace StudioFourteen.Rendering.Scene;

using System;
using System.Numerics;
using Serilog;
using SharpDX.Direct3D11;

public abstract class SceneObject : IDisposable
{
	public Transform Transform = Transform.Identity;
	public SceneObject? Parent;

	protected readonly ILogger Log;

	public SceneObject()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public ServiceManager Services => ServiceManager.Instance;
	public virtual bool IsHitTestVisible { get; set; } = true;
	public virtual bool IsVisible { get; set; } = true;

	public abstract void Draw(Transform transform, Device device, DeviceContext deviceContext);
	public abstract void Dispose();

	public abstract void HitTest(
		Vector2 screenPosition,
		Transform transform,
		Transform viewProjection,
		HitTestResult result);

	public virtual void OnHit(HitTestResult result)
	{
	}

	public virtual void PerformOnHit(HitTestResult result)
	{
		result.Handled = false;

		this.OnHit(result);

		if (result.Handled)
			return;

		if (this.Parent != null)
		{
			this.Parent.PerformOnHit(result);
		}
	}
}

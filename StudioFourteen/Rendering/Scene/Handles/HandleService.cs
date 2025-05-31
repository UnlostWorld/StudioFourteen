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

namespace StudioFourteen.Rendering.Scene.Handles;

using StudioFourteen.Services;
using System.Numerics;
using System.Runtime.CompilerServices;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Input;
using System.Windows.Input;

public partial class HandleService : ServiceBase
{
	private readonly ConditionalWeakTable<SceneObject, Handle?> parentHandles = new();
	private Handle? currentHover;

	public bool IsCursorOverHandle => this.CurrentHover != null;

	public Handle? CurrentHover
	{
		get => this.currentHover;
		set
		{
			if (this.currentHover == value)
				return;

			this.currentHover?.SetIsHandleHovered(false);
			this.currentHover = value;
			this.currentHover?.SetIsHandleHovered(true);
		}
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		base.Detach();
	}

	private void OnGameTick()
	{
		if (this.Services.Windows.IsCursorOverAtkUnit
		|| this.Services.Windows.IsCursorOverImGui
		|| this.Services.Windows.IsCursorOverStudio
		|| this.Services.Reshade.IsReshadeOverlayOpen
		|| !this.Services.Windows.IsCursorOverXiv
		|| this.Services.Input.Mouse == null)
		{
			this.CurrentHover = null;
		}
		else
		{
			/*Vector2? mousePosition = this.Services.Input.Mouse.Position.Value;

			if (mousePosition != null)
			{
				HitTestResult hitTestResult = new();

				// TODO: Scale this with resolution and aspect?
				hitTestResult.Distance = 20f / 1920f; // 20px on a 1920 monitor.

				this.Services.Rendering.Forward.HitTest(mousePosition.Value, hitTestResult);
				this.CurrentHover = this.GetHandle(hitTestResult.SceneObject);
			}

			if (this.CurrentHover != null)
			{
				bool mouseDown = this.Servicses.Input.Mouse.Buttons[MouseButton.Left].Value;
				this.CurrentHover.SetIsHandlePressed(mouseDown);
			}*/
		}
	}

	private Handle? GetHandle(SceneObject? obj)
	{
		if (obj == null)
			return null;

		if (this.parentHandles.TryGetValue(obj, out Handle? handle))
			return handle;

		handle = obj.GetParent<Handle>();
		this.parentHandles.Add(obj, handle);
		return handle;
	}
}
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
using System.Threading.Tasks;

public partial class HandleService : ServiceBase
{
	private readonly ConditionalWeakTable<SceneObject, Handle?> parentHandles = new();
	private readonly Input2DListener dragListener = new(
		InputAction.Handle_Right,
		InputAction.Handle_Left,
		InputAction.Handle_Down,
		InputAction.Handle_Up,
		"Handle Service Move");

	private readonly HitTestResult pressHitTestResult = new();
	private Handle? currentHover;
	private Handle? currentPress;
	private HandleTipWindow? handleTipWindow;

	public bool IsCursorOverHandle => this.CurrentHover != null;

	public Handle? CurrentHover
	{
		get => this.currentHover;
		set
		{
			// Pressed handles capture mouse. 🐭
			if (this.CurrentPress != null)
				value = this.CurrentPress;

			if (this.currentHover == value)
				return;

			if (this.currentHover != null)
			{
				this.currentHover.SetIsHandleHovered(false);
				this.handleTipWindow?.Hide(this.currentHover);
			}

			this.currentHover = value;

			if (this.currentHover != null)
			{
				this.currentHover.SetIsHandleHovered(true);
				this.handleTipWindow?.Show(this.currentHover);
			}
		}
	}

	public Handle? CurrentPress
	{
		get => this.currentPress;
		set
		{
			if (this.currentPress == value)
				return;

			this.currentPress?.SetIsHandlePressed(false);
			this.currentPress = value;
			this.currentPress?.SetIsHandlePressed(true);

			if (this.currentPress != null)
			{
				this.dragListener.Enable();
			}
			else
			{
				this.dragListener.Disable();
			}
		}
	}

	public override async Task Start()
	{
		this.handleTipWindow = await HandleTipWindow.CreateInstanceAsync<HandleTipWindow>();
		await base.Start();
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		this.handleTipWindow?.Hide(null);
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
			// Check hover
			if (this.CurrentPress == null)
			{
				Vector2? mousePosition = this.Services.Input.Mouse.GetPosition();
				if (mousePosition != null)
				{
					this.pressHitTestResult.Clear();

					// TODO: Scale this with resolution and aspect?
					this.pressHitTestResult.MaxDistance = 20f / 1920f; // 20px on a 1920 monitor.
					this.Services.Rendering.Forward.HitTest(mousePosition.Value, this.pressHitTestResult);
					this.CurrentHover = this.GetHandle(this.pressHitTestResult.SceneObject);
				}
			}

			// Check mouse down
			bool mouseDown = this.Services.Input.Mouse.GetButton(MouseButton.Left);
			if (this.CurrentHover != null && mouseDown)
			{
				this.CurrentPress = this.CurrentHover;
			}
			else
			{
				this.CurrentPress = null;
			}

			// Check drag
			if (this.CurrentPress != null)
			{
				Vector2 drag = this.dragListener.Value;
				this.CurrentPress.HandleDrag(this.pressHitTestResult, drag);
			}

			if (this.currentHover != null)
			{
				string toolTipContent = string.Empty;
				Vector3 toolTipPosition = Vector3.Zero;
				bool show = this.currentHover.GetToolTip(ref toolTipContent, ref toolTipPosition);
				this.handleTipWindow?.Update(show, toolTipContent, toolTipPosition);
			}
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
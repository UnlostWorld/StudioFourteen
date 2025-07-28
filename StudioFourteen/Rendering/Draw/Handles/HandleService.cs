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

namespace StudioFourteen.Rendering.Draw.Handles;

using StudioFourteen.Services;
using System.Numerics;
using System.Runtime.CompilerServices;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Input;
using System.Windows.Input;
using System.Threading.Tasks;
using System.Diagnostics;

public partial class HandleService : ServiceBase
{
	private readonly Stopwatch timeoutTimer = new();
	private readonly ConditionalWeakTable<DrawObject, Handle?> parentHandles = new();
	private readonly Input2DListener dragListener = new(
		InputAction.Handle_Right,
		InputAction.Handle_Left,
		InputAction.Handle_Down,
		InputAction.Handle_Up,
		"Handle Service Move");

	private readonly Input0DListener selectListener = new(
		InputAction.Handle_Select,
		"Handle Service Select");

	private readonly HitTestResult pressHitTestResult = new();
	private Handle? currentHover;
	private Handle? currentPress;

	private bool didDrag = false;
	////private HandleTipWindow? handleTipWindow;

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
				////this.handleTipWindow?.Hide(this.currentHover);
			}

			this.currentHover = value;

			if (this.currentHover != null)
			{
				this.currentHover.SetIsHandleHovered(true);
				////this.handleTipWindow?.Show(this.currentHover);
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
		////this.handleTipWindow = await HandleTipWindow.CreateInstanceAsync<HandleTipWindow>();
		await base.Start();
	}

	public override void Attach()
	{
		this.Services.Tick.Add(TickService.Channels.GameTick, this.OnGameTick);
		this.selectListener.Enable();
		this.Timeout();
		base.Attach();
	}

	public override void Detach()
	{
		this.Services.Tick.Remove(TickService.Channels.GameTick, this.OnGameTick);
		////this.handleTipWindow?.Hide(null);

		this.selectListener.Disable();
		base.Detach();
	}

	public void Timeout()
	{
		this.timeoutTimer.Restart();
	}

	private void OnGameTick()
	{
		if (this.Services.Input.Mouse == null)
		{
			this.CurrentPress = null;
			this.CurrentHover = null;
		}
		else if (this.CurrentPress == null &&
			(this.Services.Windows.IsCursorOverAtkUnit
			|| this.Services.Windows.IsCursorOverImGui
			|| this.Services.Windows.IsCursorOverStudio
			|| this.Services.Reshade.IsReshadeOverlayOpen
			|| !this.Services.Windows.IsCursorOverXiv))
		{
			this.CurrentHover = null;
		}
		else
		{
			bool isTimedout = true;
			if (this.timeoutTimer.ElapsedMilliseconds > 250)
			{
				this.timeoutTimer.Stop();
				isTimedout = false;
			}

			InputStates state = this.selectListener.GetState();

			// Check hover
			if (this.CurrentPress == null && !isTimedout && state != InputStates.Held)
			{
				Vector2? mousePosition = this.Services.Input.Mouse.GetPosition();
				if (mousePosition != null)
				{
					this.pressHitTestResult.Clear();

					// TODO: Scale this with resolution and aspect?
					this.pressHitTestResult.MaxDistance = 20f / 1920f; // 20px on a 1920 monitor.
					this.Services.Rendering.OverlayRenderer.Forward.HitTest(mousePosition.Value, this.pressHitTestResult);
					this.CurrentHover = this.GetHandle(this.pressHitTestResult.SceneObject);
				}
			}

			if (state == InputStates.Activated)
			{
				this.CurrentPress = this.CurrentHover;
			}

			if (state == InputStates.None)
			{
				this.CurrentPress = null;
			}

			if (state == InputStates.Deactivated && !this.didDrag)
			{
				this.Services.Selection.Clear();
			}

			if (state == InputStates.Held)
			{
				this.didDrag = this.Services.Input.Mouse.IsAnyDragging;
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

				////this.handleTipWindow?.Update(show, toolTipContent, toolTipPosition);
			}
		}
	}

	private Handle? GetHandle(DrawObject? obj)
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
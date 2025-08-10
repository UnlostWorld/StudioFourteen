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

namespace StudioFourteen.Rendering;

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using StudioFourteen.Input;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Handles;

public abstract class RendererInput
{
	private readonly Stopwatch timeoutTimer = new();
	private readonly ConditionalWeakTable<DrawObject, Handle?> parentHandles = new();
	private readonly HitTestResult pressHitTestResult = new();

	private Handle? currentHover;
	private Handle? currentPress;

	public static bool IsCursorOverHandle => GlobalHover != null;
	public static Handle? GlobalHover { get; private set; }

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
			}

			this.currentHover = value;
			GlobalHover = value;

			if (this.currentHover != null)
			{
				this.currentHover.SetIsHandleHovered(true);
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

			this.OnHandlePress(this.CurrentPress);
		}
	}

	protected ServiceManager Services => ServiceManager.Instance;

	public virtual void OnHandlePress(Handle? handle)
	{
	}

	public void Timeout()
	{
		this.timeoutTimer.Restart();
	}

	public void Process(Renderer renderer)
	{
		InputStates inputState;
		Vector2? mousePosition;
		Vector2 dragDelta;

		this.ProcessInput(out inputState, out mousePosition, out dragDelta);

		if (this.CurrentPress == null &&
			(this.Services.Windows.IsCursorOverAtkUnit
			|| this.Services.Windows.IsCursorOverImGui
			|| this.Services.Reshade.IsReshadeOverlayOpen
			|| (!this.Services.Windows.IsCursorOverXiv && !this.Services.Windows.IsCursorOverStudio)))
		{
			this.CurrentHover = null;
		}
		else
		{
			bool isTimeout = true;
			if (this.timeoutTimer.ElapsedMilliseconds <= 0 || this.timeoutTimer.ElapsedMilliseconds > 250)
			{
				this.timeoutTimer.Stop();
				isTimeout = false;
			}

			// Check hover
			if (this.CurrentPress == null && !isTimeout && inputState != InputStates.Held)
			{
				if (mousePosition != null)
				{
					this.pressHitTestResult.Clear();

					this.pressHitTestResult.MaxDistance = 20f / renderer.Width;
					renderer.HitTest(mousePosition.Value, this.pressHitTestResult);
					this.CurrentHover = this.GetHandle(this.pressHitTestResult.SceneObject);
				}
			}

			if (inputState == InputStates.Activated)
			{
				this.CurrentPress = this.CurrentHover;
			}

			if (inputState == InputStates.None)
			{
				this.CurrentPress = null;
			}

			// Check drag
			if (this.CurrentPress != null)
			{
				this.CurrentPress.HandleDrag(this.pressHitTestResult, dragDelta);
			}

			if (this.currentHover != null)
			{
				string toolTipContent = string.Empty;
				Vector3 toolTipPosition = Vector3.Zero;
				bool show = this.currentHover.GetToolTip(ref toolTipContent, ref toolTipPosition);
			}
		}
	}

	protected abstract void ProcessInput(out InputStates inputState, out Vector2? cursorPosition, out Vector2 dragDelta);

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
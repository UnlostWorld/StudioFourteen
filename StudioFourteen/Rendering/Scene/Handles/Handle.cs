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

using System.Numerics;

public abstract class Handle : SceneGroup
{
	public bool IsHovered { get; private set; }
	public bool IsPressed { get; private set; }
	public bool IsDragging { get; private set; }

	public virtual void SetIsHandleHovered(bool isHovered)
	{
		if (this.IsHovered == isHovered)
			return;

		this.OnIsHoveredChanged(isHovered);
	}

	public virtual void SetIsHandlePressed(bool isPressed)
	{
		if (this.IsPressed == isPressed)
			return;

		this.OnIsPressedChanged(isPressed);

		if (!isPressed && this.IsDragging)
		{
			this.IsDragging = false;
			this.OnEndDrag();
		}
	}

	public virtual void HandleDrag(HitTestResult initiatingHitResult, Vector2 delta)
	{
		if (!this.IsDragging)
		{
			this.IsDragging = true;
			this.OnStartDrag(initiatingHitResult);
		}

		if (delta.Length() == 0)
				return;

		this.OnDrag(delta);
	}

	protected virtual void OnIsHoveredChanged(bool isHovered)
	{
		this.IsHovered = isHovered;
	}

	protected virtual void OnIsPressedChanged(bool isPressed)
	{
		this.IsPressed = isPressed;
	}

	protected virtual void OnStartDrag(HitTestResult hitTest)
	{
	}

	protected virtual void OnDrag(Vector2 delta)
	{
	}

	protected virtual void OnEndDrag()
	{
	}
}
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

using System.Numerics;

public abstract class Handle : DrawGroup
{
	public bool IsHovered { get; private set; }
	public bool IsPressed { get; private set; }
	public bool IsDragging { get; private set; }

	public virtual bool CanDrag => true;

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
		if (!this.CanDrag)
			return;

		if (!this.IsDragging)
		{
			this.IsDragging = true;
			this.OnStartDrag(initiatingHitResult);
		}

		if (delta.Length() == 0)
			return;

		this.OnDrag(delta);
	}

	public virtual bool GetToolTip(ref string content, ref Vector3 worldPosition)
	{
		return false;
	}

	public Vector2 GetScreenPosition(Vector3 localPosition)
	{
		Transform viewProj = CameraService.CurrentView * CameraService.CurrentProjection;

		Vector4 screenPosition = new(localPosition, 1);
		screenPosition = Vector4.Transform(screenPosition, this.WorldTransform.ToMatrix());
		screenPosition = viewProj.TransformViewProjection(screenPosition);

		return screenPosition.AsVector2();
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

	protected Vector2 GetScreenVector(Vector3 localVector)
	{
		Vector2 screenPositionA = this.GetScreenPosition(Vector3.Zero);
		Vector2 screenPositionB = this.GetScreenPosition(localVector);

		Vector2 screenDir = screenPositionB - screenPositionA;
		return Vector2.Normalize(screenDir);
	}
}
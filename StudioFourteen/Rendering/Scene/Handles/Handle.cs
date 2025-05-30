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

using StudioFourteen.Input;

public abstract class Handle : SceneGroup
{
	public bool IsHovered { get; private set; }

	public virtual void SetIsHandleHovered(bool isHovered)
	{
		this.OnIsHoveredChanged(isHovered);
	}

	public virtual void OnGameTick()
	{
	}

	protected virtual void OnIsHoveredChanged(bool isHovered)
	{
		this.IsHovered = isHovered;
	}
}

public abstract class SelectableHandle : Handle
{
	private readonly InputActionListener selectListener;

	public SelectableHandle()
	{
		this.selectListener = new(InputAction.Handle_Select, "Handle Select");
	}

	public override void OnGameTick()
	{
		base.OnGameTick();

		if (this.selectListener.Value > 0.25)
		{
			this.OnSelect();
		}
	}

	protected virtual void OnSelect()
	{
	}

	protected override void OnIsHoveredChanged(bool isHovered)
	{
		base.OnIsHoveredChanged(isHovered);

		if (isHovered)
		{
			this.selectListener.Enable();
		}
		else
		{
			this.selectListener.Disable();
		}
	}
}

public abstract class DraggableHandle : Handle
{
	private readonly InputActionListener dragUpListener;
	private readonly InputActionListener dragDownListener;
	private readonly InputActionListener dragLeftListener;
	private readonly InputActionListener dragRightListener;

	public DraggableHandle()
	{
		this.dragUpListener = new(InputAction.Handle_Up, "Handle Up");
		this.dragDownListener = new(InputAction.Handle_Down, "Handle Down");
		this.dragLeftListener = new(InputAction.Handle_Left, "Handle Left");
		this.dragRightListener = new(InputAction.Handle_Right, "Handle Right");
	}

	public override void OnGameTick()
	{
		base.OnGameTick();
	}

	protected override void OnIsHoveredChanged(bool isHovered)
	{
		base.OnIsHoveredChanged(isHovered);

		if (isHovered)
		{
			this.dragUpListener.Enable();
			this.dragDownListener.Enable();
			this.dragLeftListener.Enable();
			this.dragRightListener.Enable();
		}
		else
		{
			this.dragUpListener.Disable();
			this.dragDownListener.Disable();
			this.dragLeftListener.Disable();
			this.dragRightListener.Disable();
		}
	}
}
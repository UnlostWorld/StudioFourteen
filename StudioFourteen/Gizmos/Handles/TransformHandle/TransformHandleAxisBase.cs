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

namespace StudioFourteen.Gizmos.Handles.TransformHandle;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using StudioFourteen.Gizmos.Handles;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public enum TransformHandleAxes
{
	X,
	Y,
	Z,
}

public abstract class TransformHandleAxisBase : HandleBase
{
	public Color Background = Colors.Black;
	public Color DraggingBackground = Colors.LightYellow;

	private TransformHandleBase? parent;
	private Transform? dragTransform;

	public TransformHandleAxes Axis { get; protected set; }

	protected Brush? BackgroundBrush { get; private set; }
	protected Brush? DraggingBackgroundBrush { get; private set; }

	protected double Sensitivity => this.parent?.Sensitivity ?? 1.0;

	public override void StartDrag(Point mousePos)
	{
		if (this.parent == null)
			return;

		this.dragTransform = this.parent.Transform;
		this.parent.OnAxisBeginDrag(this);
		base.StartDrag(mousePos);
	}

	public sealed override void OnDrag(Vector mouseDelta)
	{
		if (this.dragTransform == null)
			return;

		Transform transform = this.OnDrag(mouseDelta, this.dragTransform.Value);

		if (this.dragTransform != transform)
		{
			this.parent?.OnAxisDrag(transform);
			this.dragTransform = transform;
		}

		base.OnDrag(mouseDelta);
	}

	public override void EndDrag()
	{
		this.parent?.OnAxisEndDrag(this);
		base.EndDrag();
		this.dragTransform = null;
	}

	public virtual Transform OnDrag(Vector mouseDelta, Transform currentTransform)
	{
		return currentTransform;
	}

	public override void Enable(GizmoRenderer renderer)
	{
		base.Enable(renderer);
		this.BackgroundBrush = new SolidColorBrush(this.Background);
		this.DraggingBackgroundBrush = new SolidColorBrush(this.DraggingBackground);
		this.parent = this.Parent as TransformHandleBase;
	}

	public sealed override bool OnScrollWheel(float delta)
	{
		if (this.parent == null)
			return false;

		if (Keyboard.Modifiers == ModifierKeys.Shift)
			delta *= 10;

		if (Keyboard.Modifiers == ModifierKeys.Control)
			delta /= 10;

		this.dragTransform = this.parent.Transform;
		Transform transform = this.OnScrollWheel(delta, this.dragTransform.Value);

		if (this.dragTransform != transform)
		{
			this.parent?.OnAxisDrag(transform);
			this.dragTransform = transform;
		}

		return true;
	}

	public virtual Transform OnScrollWheel(float delta, Transform currentTransform)
	{
		return currentTransform;
	}
}

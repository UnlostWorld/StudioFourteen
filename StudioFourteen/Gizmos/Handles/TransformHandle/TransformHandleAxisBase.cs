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
	public Color Foreground = Colors.Gray;
	public Color Background = Colors.Black;

	private TransformHandleBase? parent;
	private Transform? dragTransform;

	public TransformHandleAxes Axis { get; protected set; }

	protected Brush? ForegroundBrush { get; private set; }
	protected Brush? BackgroundBrush { get; private set; }

	protected double Sensitivity => this.parent?.Sensitivity ?? 1.0;

	public override void StartDrag(Point mousePos)
	{
		this.dragTransform = this.parent?.OnAxisBeginDrag();
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
		base.EndDrag();
		this.dragTransform = null;
	}

	public virtual Transform OnDrag(Vector mouseDelta, Transform currentTransform)
	{
		return currentTransform;
	}

	public override void Enable(Canvas canvas)
	{
		base.Enable(canvas);

		this.ForegroundBrush = new SolidColorBrush(this.Foreground);
		this.BackgroundBrush = new SolidColorBrush(this.Background);

		this.parent = this.Parent as TransformHandleBase;
	}

	public sealed override bool OnScrollWheel(float delta)
	{
		this.dragTransform = this.parent?.OnAxisBeginDrag();

		if (this.dragTransform == null)
			return false;

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

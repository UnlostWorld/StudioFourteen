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

namespace StudioFourteen.Overlays.Gizmos;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Serilog;
using StudioFourteen;
using StudioFourteen.Overlays.Primitives;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public enum GizmoAxes
{
	X,
	Y,
	Z,
}

public abstract class GizmoAxisBase : PrimitiveBase
{
	public double Sensitivity = 1.0;
	public Color Foreground = Colors.Gray;
	public Color Background = Colors.Black;

	protected readonly ILogger Log = Logging.ForContext<GizmoAxisBase>();

	public GizmoAxisBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public GizmoAxes Axis { get; protected set; }
	public virtual bool IsAxisHovered { get; set; } = false;

	protected Brush? ForegroundBrush { get; private set; }
	protected Brush? BackgroundBrush { get; private set; }

	protected ServiceManager Services => ServiceManager.Instance;

	public abstract int GetDepthAtCursor(Point mousePos);

	public virtual void StartDrag(Point mousePos)
	{
	}

	public virtual Transform UpdateDrag(Vector mouseDelta, Transform currentTransform)
	{
		return default;
	}

	public virtual void EndDrag()
	{
	}

	public override void Enable(Canvas canvas)
	{
		base.Enable(canvas);

		this.ForegroundBrush = new SolidColorBrush(this.Foreground);
		this.BackgroundBrush = new SolidColorBrush(this.Background);
	}
}

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

namespace StudioFourteen.Gizmos;

using System.Numerics;
using System.Windows;
using System.Windows.Media;
using Serilog;

using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public enum GizmoAxes
{
	X,
	Y,
	Z,
}

public abstract class GizmoAxisBase
{
	protected readonly ILogger Log = Logging.ForContext<GizmoAxisBase>();

	public GizmoAxisBase()
	{
		this.Log = Logging.ForContext(this.GetType());
	}

	public GizmoAxes Axis { get; protected set; }
	public Brush ForegroundBrush { get; set; } = new SolidColorBrush(Colors.Gray);
	public Brush BackgroundBrush { get; set; } = new SolidColorBrush(Colors.Black);
	public bool IsAxisHovered { get; set; } = false;

	protected ServiceManager Services => ServiceManager.Instance;

	public abstract bool IsMouseOver(Point mousePos);
	public virtual void StartDrag()
	{
	}

	public virtual void UpdateDrag(Vector mouseDelta, ref Transform transform)
	{
	}

	public virtual void EndDrag()
	{
	}

	public abstract void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center);
}

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

using Serilog;
using StudioFourteen.Extensions;
using StudioFourteen.Overlays.Primitives;
using StudioFourteen.Utilities;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using CursorPoint = System.Drawing.Point;
using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

public abstract class GizmoBase : PrimitiveGroup
{
	public double Sensitivity = 1;

	protected readonly ILogger Log = Logging.ForContext<GizmoBase>();

	private readonly List<GizmoAxisBase> axes = new();
	private readonly GizmoMousePrimitive mouseHandler;

	private bool isDragging = false;
	private Point? lastDragMousePos;
	private GizmoAxisBase? hoverAxis;
	private GizmoAxisBase? dragAxis;
	private Transform? dragTransform;
	private CursorPoint cursorKeepPosition;

	public GizmoBase()
	{
		this.mouseHandler = new(this);
		this.mouseHandler.Radius = 100;

		this.AddChild(this.mouseHandler);
		this.KeepScreenSize = true;
	}

	public delegate void TransformChangedDelegate(Transform newTransform);

	public event TransformChangedDelegate? TransformChanged;

	public void OnMouseLeftButtonDown(Point mousePos)
	{
		this.OnMouseMove(mousePos);

		CursorUtility.SetCursorVisible(false);
		this.cursorKeepPosition = CursorUtility.GetPosition();

		this.isDragging = true;

		this.dragAxis = this.hoverAxis;
		if (this.dragAxis != null)
		{
			this.dragAxis.Sensitivity = this.Sensitivity;
			this.dragAxis.StartDrag(mousePos);
		}

		this.dragTransform = this.Transform;
		this.lastDragMousePos = this.cursorKeepPosition.ToWindowsPoint();
	}

	public void OnMouseLeftButtonUp(Point mousePos)
	{
		CursorUtility.SetCursorVisible(true);
		CursorUtility.SetPosition(this.cursorKeepPosition);

		this.hoverAxis?.EndDrag();

		this.isDragging = false;
		this.dragAxis = null;
		this.lastDragMousePos = null;
	}

	public void OnMouseMove(Point mousePos)
	{
		GizmoAxisBase? newHover = this.GetHoverAxis(mousePos);

		if (this.isDragging && this.dragAxis != null && this.dragTransform != null)
		{
			if (this.lastDragMousePos == null)
				this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();

			Point cursorPos = CursorUtility.GetPosition().ToWindowsPoint();
			Vector mouseDelta = cursorPos - this.lastDragMousePos.Value;
			this.lastDragMousePos = cursorPos;

			if (mouseDelta.Length > 0)
			{
				Transform newTransform = this.dragAxis.UpdateDrag(mouseDelta, this.dragTransform.Value);
				this.dragTransform = newTransform;

				if (this.dragTransform.Value != this.Transform)
				{
					this.Transform = this.dragTransform.Value;
					this.TransformChanged?.Invoke(this.Transform);
				}
			}

			// Reset cursor location
			CursorUtility.SetPosition(this.cursorKeepPosition);
			this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();
		}
		else if (newHover != this.hoverAxis)
		{
			if (this.hoverAxis != null)
			{
				this.hoverAxis.IsAxisHovered = false;
			}

			this.hoverAxis = newHover;

			if (this.hoverAxis != null)
			{
				this.hoverAxis.IsAxisHovered = true;
			}
		}
	}

	public void OnMouseLeave(Point mousePos)
	{
		CursorUtility.SetCursorVisible(true);

		if (this.hoverAxis != null)
			this.hoverAxis.IsAxisHovered = false;

		this.hoverAxis = null;
	}

	protected void AddAxis(GizmoAxisBase axis)
	{
		this.axes.Add(axis);
		this.AddChild(axis);
	}

	protected virtual GizmoAxisBase? GetHoverAxis(Point mousePos)
	{
		int highestDepthAxis = int.MinValue;
		GizmoAxisBase? bestAxis = null;

		foreach (GizmoAxisBase axis in this.axes)
		{
			int depth = axis.GetDepthAtCursor(mousePos);

			if (depth == int.MinValue)
				continue;

			if (depth > highestDepthAxis)
			{
				highestDepthAxis = depth;
				bestAxis = axis;
			}
		}

		return bestAxis;
	}
}

public class GizmoMousePrimitive : EllipsePrimitive
{
	private readonly GizmoBase gizmo;
	private Canvas? canvas;

	public GizmoMousePrimitive(GizmoBase gizmo)
	{
		this.gizmo = gizmo;
		this.Foreground = Colors.Transparent;
	}

	public override void Enable(Canvas canvas)
	{
		base.Enable(canvas);
		this.canvas = canvas;

		if (this.ellipse == null)
			return;

		this.ellipse.MouseLeftButtonDown += this.OnMouseLeftButtonDown;
		this.ellipse.MouseLeftButtonUp += this.OnMouseLeftButtonUp;
		this.ellipse.MouseMove += this.OnMouseMove;
		this.ellipse.MouseLeave += this.OnMouseLeave;
		this.ellipse.IsHitTestVisible = true;
	}

	public override void Disable(Canvas canvas)
	{
		this.canvas = null;

		if (this.ellipse != null)
		{
			this.ellipse.MouseLeftButtonDown -= this.OnMouseLeftButtonDown;
			this.ellipse.MouseLeftButtonUp -= this.OnMouseLeftButtonUp;
			this.ellipse.MouseMove -= this.OnMouseMove;
			this.ellipse.MouseLeave -= this.OnMouseLeave;
		}

		base.Disable(canvas);
	}

	private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (this.ellipse == null || this.canvas == null)
			return;

		Point mousePos = e.GetPosition(this.canvas);
		this.ellipse.CaptureMouse();
		this.gizmo.OnMouseLeftButtonDown(mousePos);
	}

	private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		if (this.ellipse == null || this.canvas == null)
			return;

		Point mousePos = e.GetPosition(this.canvas);
		this.ellipse.ReleaseMouseCapture();
		this.gizmo.OnMouseLeftButtonUp(mousePos);
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (this.ellipse == null || this.canvas == null)
			return;

		Point mousePos = e.GetPosition(this.canvas);
		this.gizmo.OnMouseMove(mousePos);
	}

	private void OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (this.ellipse == null || this.canvas == null)
			return;

		Point mousePos = e.GetPosition(this.canvas);
		this.gizmo.OnMouseLeave(mousePos);
	}
}
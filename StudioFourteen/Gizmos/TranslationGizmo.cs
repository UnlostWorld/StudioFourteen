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

using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Dalamud.Plugin.Services;
using DependencyPropertyGenerator;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using StudioFourteen.Mvm;
using StudioFourteen.Utilities;
using StudioFourteen.Extensions;

using Axis = RotationGizmo.Axis;
using CursorPoint = System.Drawing.Point;
using Vector = System.Windows.Vector;

[DependencyProperty("Translation", typeof(Vector3))]
public partial class TranslationGizmo : View
{
	private const int AxisHoverMouseDistance = 20;

	private readonly Canvas canvas;
	private readonly TranslationGizmoAxis xAxis;
	private readonly TranslationGizmoAxis yAxis;
	private readonly TranslationGizmoAxis zAxis;

	private bool isError = false;
	private bool isDragging = false;
	private Point? closestAxisMousePos = null;
	private Point? closestAxisMouseFromPos = null;
	private TranslationGizmoAxis? closestMouseAxis = null;
	private Vector3 dragTranslation;
	private Point? dragStartToPos;
	private Point? dragStartFromPos;
	private Point? lastDragMousePos;
	private TranslationGizmoAxis? dragAxis;
	private CursorPoint cursorKeepPosition;

	public TranslationGizmo()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);

		this.canvas = new();
		this.canvas.IsHitTestVisible = false;
		this.Content = this.canvas;

		this.xAxis = new(Axis.X, this.Radius, this.canvas);
		this.xAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.xAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0x33, 0xFF));

		this.yAxis = new(Axis.Y, this.Radius, this.canvas);
		this.yAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.yAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0xFF, 0x33));

		this.zAxis = new(Axis.Z, this.Radius, this.canvas);
		this.zAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.zAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0x33, 0x33));

		this.IsEnabledChanged += this.OnIsEnabledChanged;
	}

	public float Radius { get; set; } = 70;

	protected unsafe override void OnFrameworkUpdate(IFramework framework)
	{
		base.OnFrameworkUpdate(framework);

		if (!this.Services.Studio.IsOpen)
			return;

		if (this.isError)
			return;

		Camera* pCamera = CameraManager.Instance()->GetActiveCamera();
		Matrix4x4 viewMatrix = pCamera->GetViewMatrix();

		// extract just rotation from camera view
		Matrix4x4.Decompose(viewMatrix, out var _, out var rotation, out var _);
		viewMatrix = Matrix4x4.CreateFromQuaternion(rotation);

		// invert camera x
		Matrix4x4 mat = Matrix4x4.CreateScale(-1, 1, 1);
		viewMatrix = viewMatrix * mat;

		try
		{
			this.Dispatcher.Invoke(() =>
			{
				Vector3 translation = this.Translation;
				if (this.isDragging)
					translation = this.dragTranslation;

				Matrix4x4 transformMatrix = Matrix4x4.CreateTranslation(translation);
				transformMatrix.Translation = new Vector3(0, 0, 0);

				Vector2 center = default;
				center.X = (float)(this.ActualWidth / 2);
				center.Y = (float)(this.ActualHeight / 2);

				this.xAxis.Transform(transformMatrix, viewMatrix, center);
				this.yAxis.Transform(transformMatrix, viewMatrix, center);
				this.zAxis.Transform(transformMatrix, viewMatrix, center);
			});
		}
		catch (TaskCanceledException)
		{
		}
		catch (Exception ex)
		{
			this.isError = true;
			this.Log.Error(ex, "Error drawing gizmo");
		}
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		this.CaptureMouse();

		this.OnMouseMove(e);

		CursorUtility.SetCursorVisible(false);
		this.cursorKeepPosition = CursorUtility.GetPosition();

		this.isDragging = true;
		this.dragStartToPos = this.closestAxisMousePos;
		this.dragStartFromPos = this.closestAxisMouseFromPos;
		this.dragAxis = this.closestMouseAxis;
		this.dragTranslation = this.Translation;
		this.lastDragMousePos = this.cursorKeepPosition.ToWindowsPoint();
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		this.ReleaseMouseCapture();

		CursorUtility.SetCursorVisible(true);
		CursorUtility.SetPosition(this.cursorKeepPosition);

		this.isDragging = false;
		this.dragStartToPos = null;
		this.dragStartFromPos = null;
		this.dragAxis = null;
		this.lastDragMousePos = null;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		Point mousePos = e.GetPosition(this);

		double closestAxisPointToMouseDistance = double.MaxValue;

		this.xAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref this.closestAxisMousePos,
			ref this.closestAxisMouseFromPos,
			ref this.closestMouseAxis);

		this.yAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref this.closestAxisMousePos,
			ref this.closestAxisMouseFromPos,
			ref this.closestMouseAxis);

		this.zAxis.CheckAxisForMouseHover(
			mousePos,
			ref closestAxisPointToMouseDistance,
			ref this.closestAxisMousePos,
			ref this.closestAxisMouseFromPos,
			ref this.closestMouseAxis);

		if (this.isDragging && this.dragStartFromPos != null && this.dragStartToPos != null && this.dragAxis != null)
		{
			if (this.lastDragMousePos == null)
				this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();

			Point cursorPos = CursorUtility.GetPosition().ToWindowsPoint();
			Vector mouseDelta = cursorPos - this.lastDragMousePos.Value;
			this.lastDragMousePos = cursorPos;

			Vector normal = (Point)this.dragStartToPos - (Point)this.dragStartFromPos;
			normal.Normalize();

			double mag = mouseDelta.Length;
			mouseDelta.Normalize();

			double dot = Vector.Multiply(mouseDelta, normal);
			float dragDelta = (float)(mag * dot);

			if (double.IsNaN(dragDelta))
				return;

			dragDelta /= 50;

			if (Keyboard.Modifiers == ModifierKeys.Shift)
				dragDelta *= 10;

			if (Keyboard.Modifiers == ModifierKeys.Control)
				dragDelta /= 10;

			if (this.Services.Tablet.PenPressure > 0)
			{
				dragDelta *= (float)this.Services.Tablet.PenPressure;
			}

			Vector3 delta = Vector3.Zero;
			if (this.dragAxis.Axis == Axis.X)
			{
				delta = Vector3.UnitX * dragDelta;
			}
			else if (this.dragAxis.Axis == Axis.Y)
			{
				delta = Vector3.UnitY * dragDelta;
			}
			else if (this.dragAxis.Axis == Axis.Z)
			{
				delta = Vector3.UnitZ * dragDelta;
			}

			this.dragTranslation += delta;
			this.Translation = this.dragTranslation;
			e.Handled = true;

			// Reset cursor location
			CursorUtility.SetPosition(this.cursorKeepPosition);
			this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();
		}
		else if (this.closestAxisMousePos != null && this.closestMouseAxis != null && closestAxisPointToMouseDistance < AxisHoverMouseDistance)
		{
			this.closestMouseAxis.IsAxisHovered = true;
		}
		else
		{
			if (this.closestMouseAxis != null)
				this.closestMouseAxis.IsAxisHovered = false;

			this.closestAxisMousePos = null;
			this.closestMouseAxis = null;
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		CursorUtility.SetCursorVisible(true);

		base.OnMouseLeave(e);
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.Opacity = this.IsEnabled ? 1 : 0.5;
	}

	private class TranslationGizmoAxis
	{
		public readonly Axis Axis;

		private readonly Line segment;
		private readonly Line arrow;
		private readonly Vector3 segmentEnd;
		private readonly Vector3 arrowStart;

		private int strokeThickness = 3;

		public TranslationGizmoAxis(Axis axis, float radius, Canvas canvas)
		{
			this.Axis = axis;

			this.segment = new();
			this.segment.StrokeThickness = this.strokeThickness;
			this.segment.Stroke = this.ForegroundBrush;
			this.segment.StrokeEndLineCap = PenLineCap.Triangle;
			this.segment.StrokeStartLineCap = PenLineCap.Round;
			this.segment.Opacity = 0.25;
			canvas.Children.Add(this.segment);

			if (this.Axis == Axis.X)
			{
				this.segmentEnd = -Vector3.UnitX * radius;
				this.arrowStart = -Vector3.UnitX * (radius * 0.85f);
			}
			else if (this.Axis == Axis.Y)
			{
				this.segmentEnd = -Vector3.UnitY * radius;
				this.arrowStart = -Vector3.UnitY * (radius * 0.85f);
			}
			else if (this.Axis == Axis.Z)
			{
				this.segmentEnd = -Vector3.UnitZ * radius;
				this.arrowStart = -Vector3.UnitZ * (radius * 0.85f);
			}

			this.arrow = new();
			this.arrow.StrokeThickness = 15;
			this.arrow.Stroke = this.ForegroundBrush;
			this.arrow.StrokeEndLineCap = PenLineCap.Triangle;
			this.arrow.StrokeStartLineCap = PenLineCap.Round;
			canvas.Children.Add(this.arrow);
		}

		public int StrokeThickness
		{
			get => this.strokeThickness;
			set
			{
				this.strokeThickness = value;
				this.segment.StrokeThickness = value;
			}
		}

		public Brush ForegroundBrush { get; set; } = new SolidColorBrush(Colors.Gray);
		public Brush BackgroundBrush { get; set; } = new SolidColorBrush(Colors.Black);
		public bool IsAxisHovered { get; set; } = false;

		public void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
		{
			Vector3 fromPoint = Vector3.Transform(Vector3.Zero, transformMatrix);
			fromPoint = Vector3.Transform(fromPoint, viewMatrix);

			Vector3 toPoint = Vector3.Transform(this.segmentEnd, transformMatrix);
			toPoint = Vector3.Transform(toPoint, viewMatrix);

			Vector2 fromPos = center + new Vector2(fromPoint.X, fromPoint.Y);
			Vector2 toPos = center + new Vector2(toPoint.X, toPoint.Y);

			this.segment.X1 = fromPos.X;
			this.segment.Y1 = fromPos.Y;
			this.segment.X2 = toPos.X;
			this.segment.Y2 = toPos.Y;
			this.segment.StrokeThickness = this.StrokeThickness;
			this.segment.Stroke = this.ForegroundBrush;
			Panel.SetZIndex(this.segment, 200 - (int)(toPoint.Z * 100));

			Vector3 arrowFromPoint = Vector3.Transform(this.arrowStart, transformMatrix);
			arrowFromPoint = Vector3.Transform(arrowFromPoint, viewMatrix);
			Vector2 arrowFromPos = center + new Vector2(arrowFromPoint.X, arrowFromPoint.Y);
			this.arrow.X1 = arrowFromPos.X;
			this.arrow.Y1 = arrowFromPos.Y;
			this.arrow.X2 = toPos.X;
			this.arrow.Y2 = toPos.Y;
			this.arrow.Stroke = this.ForegroundBrush;
			Panel.SetZIndex(this.arrow, 200 - (int)(toPoint.Z * 100));

			if (this.IsAxisHovered)
			{
				this.arrow.StrokeThickness = 15;
			}
			else
			{
				this.arrow.StrokeThickness = 10;
			}
		}

		public void CheckAxisForMouseHover(
			Point mousePos,
			ref double closestAxisPointToMouseDistance,
			ref Point? closestAxisMousePos,
			ref Point? closestAxisMouseFromPos,
			ref TranslationGizmoAxis? closestMouseAxis)
		{
			if (!this.arrow.IsEnabled)
				return;

			Point fromPos = new Point(this.arrow.X1, this.arrow.Y1);
			Point toPos = new Point(this.arrow.X2, this.arrow.Y2);

			double distance = Point.Subtract(mousePos, toPos).Length;

			if (distance <= closestAxisPointToMouseDistance)
			{
				closestAxisPointToMouseDistance = distance;
				closestAxisMousePos = toPos;
				closestAxisMouseFromPos = fromPos;
				closestMouseAxis = this;
			}
		}
	}
}

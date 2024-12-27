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

using DependencyPropertyGenerator;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using StudioFourteen.Mvm;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using StudioFourteen.Utilities;
using StudioFourteen.Extensions;

using CursorPoint = System.Drawing.Point;
using Vector = System.Windows.Vector;

[DependencyProperty("Rotation", typeof(Quaternion), DefaultValueExpression="System.Numerics.Quaternion.Identity")]
public partial class RotationGizmo : View
{
	private const int NumPoints = 144;
	private const int AxisHoverMouseDistance = 20;

	private readonly Canvas canvas;
	private readonly Ellipse sphere;
	private readonly RotationGizmoAxis xAxis;
	private readonly RotationGizmoAxis yAxis;
	private readonly RotationGizmoAxis zAxis;
	private readonly Ellipse mousePrompt;

	private Point? closestAxisMousePos = null;
	private Point? closestAxisMouseFromPos = null;
	private RotationGizmoAxis? closestMouseAxis = null;

	private bool isError = false;
	private bool isDragging = false;
	private Point? dragStartToPos;
	private Point? dragStartFromPos;
	private Point? lastDragMousePos;
	private RotationGizmoAxis? dragAxis;
	private CursorPoint cursorKeepPosition;

	private Quaternion dragRotation;

	public RotationGizmo()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);

		this.canvas = new();
		this.canvas.IsHitTestVisible = false;
		this.Content = this.canvas;

		this.sphere = new();
		this.sphere.Width = this.Radius * 2;
		this.sphere.Height = this.Radius * 2;
		this.sphere.Fill = new SolidColorBrush(Color.FromArgb(0x50, 0, 0, 0));
		this.canvas.Children.Add(this.sphere);
		Panel.SetZIndex(this.sphere, 0);

		this.xAxis = new(GizmoAxes.X, this.Radius, this.canvas);
		this.xAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0x33, 0xFF));
		this.xAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0x33, 0xFF));

		this.yAxis = new(GizmoAxes.Y, this.Radius, this.canvas);
		this.yAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0x33, 0xFF, 0x33));
		this.yAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0x33, 0xFF, 0x33));

		this.zAxis = new(GizmoAxes.Z, this.Radius, this.canvas);
		this.zAxis.ForegroundBrush = new SolidColorBrush(Color.FromArgb(0xFF, 0xFF, 0x33, 0x33));
		this.zAxis.BackgroundBrush = new SolidColorBrush(Color.FromArgb(0x10, 0xFF, 0x33, 0x33));

		this.mousePrompt = new();
		this.mousePrompt.Width = 10;
		this.mousePrompt.Height = 10;
		this.canvas.Children.Add(this.mousePrompt);
		Panel.SetZIndex(this.mousePrompt, 10000);

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
				Quaternion rot = this.Rotation;
				if (this.isDragging)
					rot = this.dragRotation;

				Matrix4x4 transformMatrix = Matrix4x4.CreateFromQuaternion(rot);
				transformMatrix.Translation = new Vector3(0, 0, 0);

				Vector2 center = default;
				center.X = (float)(this.ActualWidth / 2);
				center.Y = (float)(this.ActualHeight / 2);

				Canvas.SetLeft(this.sphere, center.X - (this.sphere.Width / 2));
				Canvas.SetTop(this.sphere, center.Y - (this.sphere.Height / 2));

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

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		if (this.closestAxisMousePos != null && this.closestMouseAxis != null)
		{
			float mouseWheel = e.Delta / 1000.0f;

			if (Keyboard.Modifiers == ModifierKeys.Shift)
				mouseWheel *= 10;

			if (Keyboard.Modifiers == ModifierKeys.Control)
				mouseWheel /= 10;

			Quaternion rot = Quaternion.Identity;
			if (this.closestMouseAxis.Axis == GizmoAxes.X)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, mouseWheel);
			}
			else if (this.closestMouseAxis.Axis == GizmoAxes.Y)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, -mouseWheel);
			}
			else if (this.closestMouseAxis.Axis == GizmoAxes.Z)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, mouseWheel);
			}

			this.Rotation = this.Rotation * rot;
			e.Handled = true;
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
		this.dragRotation = this.Rotation;
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

			double angleChange = dragDelta / 50;

			if (Keyboard.Modifiers == ModifierKeys.Shift)
				angleChange *= 10;

			if (Keyboard.Modifiers == ModifierKeys.Control)
				angleChange /= 10;

			if (this.Services.Tablet.PenPressure > 0)
			{
				angleChange *= this.Services.Tablet.PenPressure;
			}

			Quaternion rot = Quaternion.Identity;
			if (this.dragAxis.Axis == GizmoAxes.X)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float)angleChange);
			}
			else if (this.dragAxis.Axis == GizmoAxes.Y)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)-angleChange);
			}
			else if (this.dragAxis.Axis == GizmoAxes.Z)
			{
				rot = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float)angleChange);
			}

			this.dragRotation = this.dragRotation * rot;
			this.Rotation = this.dragRotation;
			e.Handled = true;

			// Reset cursor location
			CursorUtility.SetPosition(this.cursorKeepPosition);
			this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();
		}
		else if (this.closestAxisMousePos != null && this.closestMouseAxis != null && closestAxisPointToMouseDistance < AxisHoverMouseDistance)
		{
			this.mousePrompt.Visibility = Visibility.Visible;
			this.mousePrompt.Fill = this.closestMouseAxis.ForegroundBrush;

			Canvas.SetLeft(this.mousePrompt, this.closestAxisMousePos.Value.X - (this.mousePrompt.Width / 2));
			Canvas.SetTop(this.mousePrompt, this.closestAxisMousePos.Value.Y - (this.mousePrompt.Height / 2));
		}
		else
		{
			this.mousePrompt.Visibility = Visibility.Collapsed;
			this.closestAxisMousePos = null;
			this.closestMouseAxis = null;
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.mousePrompt.Visibility = Visibility.Collapsed;
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.Opacity = this.IsEnabled ? 1 : 0.5;
	}

	private class RotationGizmoAxis
	{
		public readonly GizmoAxes Axis;

		private readonly Line[] segments = new Line[NumPoints];
		private readonly Vector3[] points3d = new Vector3[NumPoints];
		private int strokeThickness = 3;

		public RotationGizmoAxis(GizmoAxes axis, float radius, Canvas canvas)
		{
			this.Axis = axis;

			for (int i = 0; i < this.points3d.Length; i++)
			{
				float p = i / (float)(this.points3d.Length - 1);
				float r = p * (MathF.PI * 2);

				if (axis == GizmoAxes.Z)
				{
					this.points3d[i] = new Vector3(radius * MathF.Cos(r), radius * MathF.Sin(r), 0);
				}
				else if (axis == GizmoAxes.X)
				{
					this.points3d[i] = new Vector3(0, radius * MathF.Cos(r), radius * MathF.Sin(r));
				}
				else if (axis == GizmoAxes.Y)
				{
					this.points3d[i] = new Vector3(radius * MathF.Cos(r), 0, radius * MathF.Sin(r));
				}
			}

			for (int i = 1; i < this.segments.Length; i++)
			{
				this.segments[i] = new();
				this.segments[i].StrokeThickness = this.strokeThickness;
				this.segments[i].Stroke = this.ForegroundBrush;
				this.segments[i].StrokeEndLineCap = PenLineCap.Round;
				this.segments[i].StrokeStartLineCap = PenLineCap.Round;
				canvas.Children.Add(this.segments[i]);
			}
		}

		public int StrokeThickness
		{
			get => this.strokeThickness;
			set
			{
				this.strokeThickness = value;
				foreach (Line line in this.segments)
				{
					line.StrokeThickness = value;
				}
			}
		}

		public Brush ForegroundBrush { get; set; } = new SolidColorBrush(Colors.White);
		public Brush BackgroundBrush { get; set; } = new SolidColorBrush(Colors.Black);

		public void Transform(Matrix4x4 transformMatrix, Matrix4x4 viewMatrix, Vector2 center)
		{
			float zClip = 0f;
			for (int i = 1; i < this.points3d.Length; i++)
			{
				Vector3 fromPoint = Vector3.Transform(this.points3d[i - 1], transformMatrix);
				fromPoint = Vector3.Transform(fromPoint, viewMatrix);

				Vector3 toPoint = Vector3.Transform(this.points3d[i], transformMatrix);
				toPoint = Vector3.Transform(toPoint, viewMatrix);

				bool isVisible = toPoint.Z < zClip;
				Vector2 fromPos = center + new Vector2(fromPoint.X, fromPoint.Y);
				Vector2 toPos = center + new Vector2(toPoint.X, toPoint.Y);

				Line line = this.segments[i];
				line.X1 = fromPos.X;
				line.Y1 = fromPos.Y;
				line.X2 = toPos.X;
				line.Y2 = toPos.Y;
				line.IsEnabled = isVisible;

				Panel.SetZIndex(line, isVisible ? 200 : 100);

				line.StrokeThickness = this.StrokeThickness;
				line.Stroke = isVisible ? this.ForegroundBrush : this.BackgroundBrush;
			}
		}

		public void CheckAxisForMouseHover(
			Point mousePos,
			ref double closestAxisPointToMouseDistance,
			ref Point? closestAxisMousePos,
			ref Point? closestAxisMouseFromPos,
			ref RotationGizmoAxis? closestMouseAxis)
		{
			for (int i = 1; i < this.points3d.Length; i++)
			{
				Line segment = this.segments[i];

				if (!segment.IsEnabled)
					continue;

				Point fromPos = new Point(segment.X1, segment.Y1);
				Point toPos = new Point(segment.X2, segment.Y2);

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
}
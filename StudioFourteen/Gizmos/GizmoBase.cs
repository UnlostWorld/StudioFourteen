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

using Dalamud.Plugin.Services;
using StudioFourteen.Mvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using System.Numerics;
using StudioFourteen.Extensions;
using StudioFourteen.Utilities;
using System.Windows.Input;
using DependencyPropertyGenerator;

using CursorPoint = System.Drawing.Point;
using Transform = StudioFourteen.Posing.Transform;
using Vector = System.Windows.Vector;

[DependencyProperty<double>("Sensitivity", DefaultValue=1)]
[DependencyProperty<Transform>("Transform", DefaultBindingMode=DefaultBindingMode.TwoWay)]
public abstract partial class GizmoBase : View
{
	protected readonly Canvas Canvas;

	private readonly List<GizmoAxisBase> axes = new();

	private bool isError = false;
	private bool isDragging = false;
	private bool isLoading = false;
	private Point? lastDragMousePos;
	private GizmoAxisBase? hoverAxis;
	private GizmoAxisBase? dragAxis;
	private Transform? dragTransform;
	private CursorPoint cursorKeepPosition;

	public GizmoBase()
	{
		this.Background = new SolidColorBrush(Colors.Transparent);

		this.Canvas = new();
		this.Canvas.IsHitTestVisible = false;
		this.Content = this.Canvas;

		this.IsEnabledChanged += this.OnIsEnabledChanged;
		this.IsVisibleChanged += this.OnIsVisibleChanged;
	}

	protected void AddAxis(GizmoAxisBase axis)
	{
		this.axes.Add(axis);
	}

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
				Transform transform = this.Transform;
				if (this.isDragging && this.dragTransform != null)
					transform = this.dragTransform.Value;

				Matrix4x4 transformMatrix = this.GetTransformMatrix(transform);

				Vector2 center = default;
				center.X = (float)(this.ActualWidth / 2);
				center.Y = (float)(this.ActualHeight / 2);

				foreach(var axis in this.axes)
				{
					axis.Transform(transformMatrix, viewMatrix, center);
				}

				this.OnDraw(center);

				if (this.isLoading)
				{
					this.isLoading = false;
					this.Opacity = 1.0f;
				}
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

	protected virtual Matrix4x4 GetTransformMatrix(Transform transform)
	{
		Matrix4x4 transformMatrix = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(transform.Rotation));
		return transformMatrix;
		////return transform.ToMatrix();
	}

	protected virtual void OnDraw(Vector2 center)
	{
	}

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		this.CaptureMouse();

		this.OnMouseMove(e);

		CursorUtility.SetCursorVisible(false);
		this.cursorKeepPosition = CursorUtility.GetPosition();

		this.isDragging = true;

		this.dragAxis = this.hoverAxis;
		if (this.dragAxis != null)
		{
			Point mousePos = e.GetPosition(this);
			this.dragAxis.Sensitivity = this.Sensitivity;
			this.dragAxis.StartDrag(mousePos);
		}

		this.dragTransform = this.Transform;
		this.lastDragMousePos = this.cursorKeepPosition.ToWindowsPoint();
	}

	protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonUp(e);
		this.ReleaseMouseCapture();

		CursorUtility.SetCursorVisible(true);
		CursorUtility.SetPosition(this.cursorKeepPosition);

		this.hoverAxis?.EndDrag();

		this.isDragging = false;
		this.dragAxis = null;
		this.lastDragMousePos = null;
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		Point mousePos = e.GetPosition(this);
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
				this.Transform = this.dragTransform.Value;
			}

			e.Handled = true;

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

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		CursorUtility.SetCursorVisible(true);

		base.OnMouseLeave(e);
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible)
		{
			this.Opacity = 0;
			this.isLoading = true;
		}
	}

	private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.Opacity = this.IsEnabled ? 1 : 0.5;
	}
}

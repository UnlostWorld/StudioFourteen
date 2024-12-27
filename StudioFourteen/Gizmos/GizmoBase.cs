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

[DependencyProperty<Transform>("Transform", DefaultBindingMode=DefaultBindingMode.TwoWay)]
public abstract partial class GizmoBase : View
{
	protected readonly Canvas Canvas;

	private readonly List<GizmoAxisBase> axes = new();

	private bool isError = false;
	private bool isDragging = false;
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

				Matrix4x4 transformMatrix = Matrix4x4.CreateFromQuaternion(transform.Rotation);
				transformMatrix.Translation = new Vector3(0, 0, 0);

				Vector2 center = default;
				center.X = (float)(this.ActualWidth / 2);
				center.Y = (float)(this.ActualHeight / 2);

				foreach(var axis in this.axes)
				{
					axis.Transform(transformMatrix, viewMatrix, center);
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

	protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
	{
		base.OnMouseLeftButtonDown(e);
		this.CaptureMouse();

		this.OnMouseMove(e);

		CursorUtility.SetCursorVisible(false);
		this.cursorKeepPosition = CursorUtility.GetPosition();

		this.isDragging = true;

		this.hoverAxis?.StartDrag();

		this.dragAxis = this.hoverAxis;
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

		GizmoAxisBase? newHover = null;
		foreach (GizmoAxisBase axis in this.axes)
		{
			if (axis.IsMouseOver(mousePos))
			{
				newHover = axis;
			}
		}

		if (this.isDragging && this.dragAxis != null && this.dragTransform != null)
		{
			if (this.lastDragMousePos == null)
				this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();

			Point cursorPos = CursorUtility.GetPosition().ToWindowsPoint();
			Vector mouseDelta = cursorPos - this.lastDragMousePos.Value;
			this.lastDragMousePos = cursorPos;

			Transform deltaTransform = this.dragTransform.Value;
			this.dragAxis.UpdateDrag(mouseDelta, ref deltaTransform);
			this.dragTransform = deltaTransform;
			this.Transform = this.dragTransform.Value;
			e.Handled = true;

			// Reset cursor location
			CursorUtility.SetPosition(this.cursorKeepPosition);
			this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();
		}
		else if (newHover != null && this.hoverAxis == null)
		{
			this.hoverAxis = newHover;
			this.hoverAxis.IsAxisHovered = true;
		}
		else if (newHover == null && this.hoverAxis != null)
		{
			this.hoverAxis.IsAxisHovered = false;
			this.hoverAxis = null;
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
}

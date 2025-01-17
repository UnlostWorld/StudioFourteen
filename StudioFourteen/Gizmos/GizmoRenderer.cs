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
using Serilog;
using StudioFourteen.Extensions;
using StudioFourteen.Gizmos.Handles;
using StudioFourteen.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WpfUtils;
using WpfUtils.Extensions;

using CursorPoint = System.Drawing.Point;
using Vector = System.Windows.Vector;

public class GizmoRenderer : Canvas
{
	public readonly List<IGizmo> Primitives = new();
	private CursorPoint cursorKeepPosition;
	private HandleBase? draggingHandle;
	private HandleBase? cursorOverHandle;
	private Point? lastDragMousePos;
	private bool isAnyMouseDown;

	public GizmoRenderer()
	{
		this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));
		this.IsVisibleChanged += this.OnIsVisibleChanged;
	}

	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public void AddPrimitive(IGizmo primitive)
	{
		this.Dispatcher.Invoke(() =>
		{
			primitive.Enable(this);
		});

		this.Primitives.Add(primitive);
	}

	public void RemovePrimitive(IGizmo primitive)
	{
		this.Dispatcher.Invoke(() =>
		{
			primitive.Disable(this);
		});

		this.Primitives.Remove(primitive);
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		this.isAnyMouseDown = true;

		if (this.cursorOverHandle != null)
		{
			this.CaptureMouse();

			CursorUtility.SetCursorVisible(false);
			this.cursorKeepPosition = CursorUtility.GetPosition();

			this.draggingHandle = this.cursorOverHandle;
			if (this.draggingHandle != null)
			{
				Point mousePos = e.GetPosition(this);
				this.draggingHandle.StartDrag(mousePos);
				e.Handled = true;
			}

			this.lastDragMousePos = this.cursorKeepPosition.ToWindowsPoint();

			e.Handled = true;
		}
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		this.isAnyMouseDown = false;

		if (e.Handled)
			return;

		if (this.draggingHandle != null)
		{
			Point mousePos = e.GetPosition(this);
			this.ReleaseMouseCapture();

			CursorUtility.SetCursorVisible(true);
			CursorUtility.SetPosition(this.cursorKeepPosition);

			this.cursorOverHandle?.EndDrag();

			this.draggingHandle = null;
			this.lastDragMousePos = null;

			e.Handled = true;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		if (this.draggingHandle != null)
		{
			if (this.lastDragMousePos == null)
				this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();

			Point cursorPos = CursorUtility.GetPosition().ToWindowsPoint();
			Vector mouseDelta = cursorPos - this.lastDragMousePos.Value;
			this.lastDragMousePos = cursorPos;

			if (mouseDelta.Length > 0)
				this.draggingHandle.OnDrag(mouseDelta);

			// Reset cursor location
			CursorUtility.SetPosition(this.cursorKeepPosition);
			this.lastDragMousePos = CursorUtility.GetPosition().ToWindowsPoint();

			e.Handled = true;
		}
		else if (!this.isAnyMouseDown)
		{
			Point mousePos = e.GetPosition(this);
			HandleBase? newHover = this.HitTest(mousePos);

			if (newHover != this.cursorOverHandle)
			{
				this.cursorOverHandle?.SetIsCursorOver(false);
				this.cursorOverHandle = newHover;
				this.cursorOverHandle?.SetIsCursorOver(true);

				e.Handled = true;
			}
		}
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);

		this.isAnyMouseDown = false;

		CursorUtility.SetCursorVisible(true);

		this.cursorOverHandle?.SetIsCursorOver(false);
		this.cursorOverHandle = null;
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		if (this.cursorOverHandle != null)
		{
			if (this.cursorOverHandle.OnScrollWheel(e.Delta / 120.0f))
			{
				e.Handled = true;
			}
		}
	}

	protected virtual Matrix4x4 GetViewMatrix() => this.Services.Camera.CurrentView;
	protected virtual Matrix4x4 GetProjectionMatrix() => this.Services.Camera.CurrentProjection;

	protected virtual HandleBase? HitTest(Point mousePos)
	{
		HandleHitResult result = default;
		foreach (IGizmo primitive in this.Primitives)
		{
			if (!primitive.IsVisible)
				continue;

			primitive.HitTest(mousePos, ref result);
		}

		return result.Handle;
	}

	private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this.IsVisible)
		{
			this.RenderTask().Run();
		}
	}

	private async Task RenderTask()
	{
		try
		{
			Stopwatch sw = new();

			await this.MainThread();
			while (this.IsVisible && !StudioFourteen.Services.ServiceManagerBase.ShutdownRequested)
			{
				long delay = 16 - sw.ElapsedMilliseconds;
				await Task.Delay(int.Max((int)delay, 1));
				sw.Restart();

				await this.MainThread();

				if (!this.IsVisible || StudioFourteen.Services.ServiceManagerBase.ShutdownRequested)
					return;

				Matrix4x4 view = this.GetViewMatrix();
				Matrix4x4 projection = this.GetProjectionMatrix();

				for (int i = this.Primitives.Count - 1; i >= 0; i--)
				{
					IGizmo primitive = this.Primitives[i];

					try
					{
						primitive.Update(view, projection, this);
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Error in primitive transform {primitive}");
					}
				}
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in primitive renderer");
		}
	}
}

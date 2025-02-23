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
using StudioFourteen.Mvm;
using StudioFourteen.Plugin;
using StudioFourteen.Structs;
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

using Vector = System.Windows.Vector;

public class GizmoRenderer : Canvas
{
	public readonly List<IGizmo> Gizmos = new();
	private HandleBase? draggingHandle;
	private HandleBase? cursorOverHandle;
	private Point? lastDragMousePos;
	private Vector? cursorDragOffset;
	private bool isAnyMouseDown;

	public GizmoRenderer()
	{
		this.Background = new SolidColorBrush(Color.FromArgb(1, 0, 0, 0));

		this.Loaded += (s, e) =>
		{
			try
			{
				this.OnLoaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error loading gizmo renderer");
			}
		};

		this.Unloaded += (s, e) =>
		{
			try
			{
				this.OnUnloaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error unloading gizmo renderer");
			}
		};

		this.Dispatcher.ShutdownStarted += (s, e) =>
		{
			try
			{
				this.OnUnloaded();
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error unloading gizmo renderer");
			}
		};
	}

	public bool IsRendererLoaded { get; private set; }

	public ILogger Log => Logging.ForContext(this.GetType());
	public ServiceManager Services => ServiceManager.Instance;

	public void AddGizmo(IGizmo gizmo)
	{
		this.Dispatcher.Invoke(() =>
		{
			if (gizmo.GetIsVisible())
			{
				try
				{
					gizmo.Enable(this);
				}
				catch (Exception ex)
				{
					this.Log.Error(ex, "Error enabling gizmo");
					this.Gizmos.Remove(gizmo);
				}
			}
		});

		this.Gizmos.Add(gizmo);
	}

	public void RemoveGizmo(IGizmo gizmo)
	{
		this.Dispatcher.Invoke(() =>
		{
			try
			{
				gizmo.Disable(this);
			}
			catch (Exception ex)
			{
				this.Log.Error(ex, "Error disabling gizmo");
			}
		});

		this.Gizmos.Remove(gizmo);
	}

	protected virtual void OnLoaded()
	{
		this.IsRendererLoaded = true;

		this.RenderTask().Run();
	}

	protected virtual void OnUnloaded()
	{
		this.IsRendererLoaded = false;
		this.Children.Clear();
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		this.isAnyMouseDown = true;

		if (this.cursorOverHandle != null)
		{
			this.CaptureMouse();

			CursorUtility.SetCursorVisible(false);
			Point cursorPosition = CursorUtility.GetPosition();

			this.draggingHandle = this.cursorOverHandle;
			if (this.draggingHandle != null)
			{
				Point mousePos = e.GetPosition(this);
				this.draggingHandle.StartDrag(mousePos);

				Vector3 handleScreenPosition = this.draggingHandle.LocalToScreen(Vector3.Zero);
				this.cursorDragOffset = cursorPosition - handleScreenPosition.ToPoint();

				e.Handled = true;
			}

			this.lastDragMousePos = cursorPosition;

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

			if (this.cursorDragOffset != null)
			{
				Vector3 handleScreenPosition = this.draggingHandle.LocalToScreen(Vector3.Zero);
				CursorUtility.SetPosition(handleScreenPosition.ToPoint() + this.cursorDragOffset.Value);
			}

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
				this.lastDragMousePos = CursorUtility.GetPosition();

			Point cursorPos = CursorUtility.GetPosition();
			Vector mouseDelta = cursorPos - this.lastDragMousePos.Value;
			this.lastDragMousePos = cursorPos;

			if (mouseDelta.Length > 0)
				this.draggingHandle.OnDrag(mouseDelta);

			// Reset cursor location
			if (this.cursorDragOffset != null)
			{
				Vector3 handleScreenPosition = this.draggingHandle.LocalToScreen(Vector3.Zero);
				CursorUtility.SetPosition(handleScreenPosition.ToPoint() + this.cursorDragOffset.Value);
			}

			this.lastDragMousePos = CursorUtility.GetPosition();

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
		if (!this.IsRendererLoaded)
			return null;

		HandleHitResult result = default;
		foreach (IGizmo gizmo in this.Gizmos)
		{
			try
			{
				if (!gizmo.IsVisible)
					continue;

				gizmo.HitTest(mousePos, ref result);
			}
			catch(Exception ex)
			{
				this.Log.Error(ex, "Error in gizmo Hit Test");
				this.Gizmos.Remove(gizmo);
				break;
			}
		}

		return result.Handle;
	}

	private async Task RenderTask()
	{
		try
		{
			Stopwatch sw = new();

			await this.MainThread();
			while (this.IsRendererLoaded)
			{
				Matrix4x4 view = this.GetViewMatrix();
				Matrix4x4 projection = this.GetProjectionMatrix();

				for (int i = this.Gizmos.Count - 1; i >= 0; i--)
				{
					IGizmo gizmo = this.Gizmos[i];

					if (!gizmo.IsEnabled)
						continue;

					try
					{
						gizmo.Update(view, projection, this);
					}
					catch (Exception ex)
					{
						this.Log.Error(ex, $"Error in gizmo update {gizmo}");
						this.Gizmos.Remove(gizmo);
						break;
					}
				}

				long delay = 16 - sw.ElapsedMilliseconds;
				await Task.Delay(int.Max((int)delay, 1));
				sw.Restart();

				await this.MainThread();
			}
		}
		catch (Exception ex)
		{
			this.Log.Error(ex, $"Error in gizmo renderer");
		}

		await this.MainThread();
		foreach (IGizmo gizmo in this.Gizmos)
		{
			gizmo.Disable(this);
		}

		this.Gizmos.Clear();
	}
}

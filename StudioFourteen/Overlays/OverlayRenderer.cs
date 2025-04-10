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

namespace StudioFourteen.Overlays;

using StudioFourteen.Gizmos;
using System.Windows.Input;
using WpfUtils.Extensions;

public partial class OverlayRenderer : GizmoRenderer
{
	protected override void OnLoaded()
	{
		base.OnLoaded();

		this.Services.Overlays.LayerAdded += this.OnLayerAdded;
		this.Services.Overlays.LayerRemoved += this.OnLayerRemoved;
		this.Services.Overlays.ShowOverlaysChanged += this.OnShowOverlaysChanged;

		foreach (OverlayLayerBase layer in this.Services.Overlays.GetOverlayLayers().ToArray())
		{
			this.OnLayerAdded(layer);
		}
	}

	protected override void OnUnloaded()
	{
		base.OnUnloaded();

		this.Services.Overlays.LayerAdded -= this.OnLayerAdded;
		this.Services.Overlays.LayerRemoved -= this.OnLayerRemoved;
		this.Services.Overlays.ShowOverlaysChanged -= this.OnShowOverlaysChanged;

		foreach (OverlayLayerBase layer in this.Services.Overlays.GetOverlayLayers())
		{
			this.OnLayerRemoved(layer);
		}
	}

	protected override void OnMouseDown(MouseButtonEventArgs e)
	{
		base.OnMouseDown(e);

		if (e.Handled)
			return;

		this.Services.Input.Mouse?.HandleMouse(e.ChangedButton, true);
	}

	protected override void OnMouseUp(MouseButtonEventArgs e)
	{
		base.OnMouseUp(e);

		if (e.Handled)
			return;

		if (this.Services.Input.Mouse?.IsAnyDragging == false)
		{
			if (e.ChangedButton == MouseButton.Right)
			{
				////this.WorldContextMenu.Show(e.GetPosition(this));
			}
			else if (e.ChangedButton == MouseButton.Left)
			{
				this.Services.Target.TargetPosition(e.GetPosition(this)).Run();
			}
		}

		this.Services.Input.Mouse?.HandleMouse(e.ChangedButton, false);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);

		this.Services.Input.Mouse?.HandleMouseMove();
	}

	protected override void OnMouseLeave(MouseEventArgs e)
	{
		base.OnMouseLeave(e);
		this.Services.Input.Mouse?.HandleMouseLeave();
	}

	protected override void OnMouseWheel(MouseWheelEventArgs e)
	{
		base.OnMouseWheel(e);

		if (e.Handled)
			return;

		this.Services.Input.Mouse?.HandleMouseWheel(e.Delta / 120.0f);
	}

	private void OnLayerAdded(OverlayLayerBase overlay)
	{
		if (!this.Services.Overlays.ShowOverlays)
			return;

		this.AddGizmo(overlay);
	}

	private void OnLayerRemoved(OverlayLayerBase overlay)
	{
		this.RemoveGizmo(overlay);
	}

	private void OnShowOverlaysChanged(bool state)
	{
		foreach (OverlayLayerBase layer in this.Services.Overlays.GetOverlayLayers())
		{
			if (state)
			{
				this.OnLayerAdded(layer);
			}
			else
			{
				this.OnLayerRemoved(layer);
			}
		}
	}
}

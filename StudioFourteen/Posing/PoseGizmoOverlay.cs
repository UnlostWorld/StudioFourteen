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

namespace StudioFourteen.Posing;

using StudioFourteen.Gizmos;
using StudioFourteen.Overlays;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

public abstract class PoseOverlayBase : OverlayBase
{
	protected SelectionBase? selection;

	public PoseOverlayBase(string name)
		: base("Posing", name)
	{
	}

	public void SetTarget(int targetIndex)
	{
	}

	public void SetSelection(SelectionBase? selection)
	{
		this.selection = selection;
	}
}

public class PoseGizmoOverlay : PoseOverlayBase
{
	private readonly GizmoTypes gizmoType = GizmoTypes.Rotation;

	private TranslationGizmo? translation;
	private RotationGizmo? rotation;
	private ScaleGizmo? scale;

	public PoseGizmoOverlay()
		: base("Gizmo")
	{
	}

	public override void Initialize(Canvas canvas)
	{
		base.Initialize(canvas);

		this.translation = new();
		this.rotation = new();
		this.scale = new();

		canvas.Children.Add(this.translation);
		canvas.Children.Add(this.rotation);
		canvas.Children.Add(this.scale);
	}

	public override void Shutdown(Canvas canvas)
	{
		base.Shutdown(canvas);

		if (this.translation != null)
			canvas.Children.Remove(this.translation);

		if (this.rotation != null)
			canvas.Children.Remove(this.rotation);

		if (this.scale != null)
			canvas.Children.Remove(this.scale);
	}

	public override void Update(Canvas canvas)
	{
		if (this.selection is not TransformSelectionBase transformSelection)
			return;

		if (this.translation == null
			|| this.rotation == null
			|| this.scale == null)
			return;

		Transform worldTransform = transformSelection.WorldTransform;
		this.Services.Camera.WorldToCamera(worldTransform.Translation, out Vector3 pos);

		this.translation.Transform = worldTransform;
		this.translation.Visibility = this.gizmoType == GizmoTypes.Translation ? Visibility.Visible : Visibility.Collapsed;
		Canvas.SetLeft(this.translation, pos.X * canvas.ActualWidth);
		Canvas.SetTop(this.translation, pos.Y * canvas.ActualHeight);

		this.rotation.Transform = worldTransform;
		this.rotation.Visibility = this.gizmoType == GizmoTypes.Rotation ? Visibility.Visible : Visibility.Collapsed;
		Canvas.SetLeft(this.rotation, pos.X * canvas.ActualWidth);
		Canvas.SetTop(this.rotation, pos.Y * canvas.ActualHeight);

		this.scale.Transform = worldTransform;
		this.scale.Visibility = this.gizmoType == GizmoTypes.Scale ? Visibility.Visible : Visibility.Collapsed;
		Canvas.SetLeft(this.scale, pos.X * canvas.ActualWidth);
		Canvas.SetTop(this.scale, pos.Y * canvas.ActualHeight);
	}
}

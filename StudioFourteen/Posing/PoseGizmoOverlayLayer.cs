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

using StudioFourteen.Overlays;
using StudioFourteen.Overlays.Gizmos;
using StudioFourteen.Overlays.Gizmos.Rotation;
using StudioFourteen.Overlays.Gizmos.Scale;
using StudioFourteen.Overlays.Gizmos.Translation;

public abstract class PoseOverlayLayerBase : OverlayLayerBase
{
	protected SelectionBase? selection;

	public PoseOverlayLayerBase(string name)
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

public class PoseGizmoOverlayLayer : PoseOverlayLayerBase
{
	private readonly TranslationGizmo translation;
	private readonly RotationGizmo rotation;
	private readonly ScaleGizmo scale;

	public PoseGizmoOverlayLayer()
		: base("Gizmo")
	{
		this.translation = new();
		this.translation.Flip = true;
		this.translation.WriteTransform = false;
		this.translation.TransformChanged += this.OnTransformChanged;
		this.AddChild(this.translation);

		this.rotation = new();
		this.rotation.TransformChanged += this.OnTransformChanged;
		this.AddChild(this.rotation);

		this.scale = new();
		this.scale.TransformChanged += this.OnTransformChanged;
		this.AddChild(this.scale);
	}

	public unsafe override void OnFrameworkUpdate()
	{
		base.OnFrameworkUpdate();

		if (this.selection is not TransformSelectionBase transformSelection)
			return;

		this.translation.IsVisible = this.Services.Pose.Gizmo == GizmoTypes.Translation;
		this.translation.Transform = transformSelection.WorldTransform;

		this.rotation.IsVisible = this.Services.Pose.Gizmo == GizmoTypes.Rotation;
		this.rotation.Transform = transformSelection.WorldTransform;

		this.scale.IsVisible = this.Services.Pose.Gizmo == GizmoTypes.Scale;
		this.scale.Transform = transformSelection.WorldTransform;
	}

	private void OnTransformChanged(Transform newTransform)
	{
		if (this.selection is not TransformSelectionBase transformSelection)
			return;

		this.translation.Transform = transformSelection.WorldTransform;
		transformSelection.WorldTransform = newTransform;
	}
}

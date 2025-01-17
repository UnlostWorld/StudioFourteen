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

using StudioFourteen.Gizmos.Handles.TransformHandle;
using StudioFourteen.Gizmos.Handles.TransformHandle.Rotation;
using StudioFourteen.Gizmos.Handles.TransformHandle.Scale;
using StudioFourteen.Gizmos.Handles.TransformHandle.Translation;
using StudioFourteen.Overlays;

public abstract class PoseOverlayLayerBase : OverlayLayerBase
{
	protected SelectionBase? selection;

	public PoseOverlayLayerBase(string name)
		: base("Posing", name)
	{
	}

	public virtual void SetTarget(int targetIndex)
	{
	}

	public virtual void SetSelection(SelectionBase? selection)
	{
		this.selection = selection;
	}
}

public class PoseTransformHandleOverlayLayer : PoseOverlayLayerBase
{
	private readonly TranslationHandle translation;
	private readonly RotationHandle rotation;
	private readonly ScaleHandle scale;

	public PoseTransformHandleOverlayLayer()
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

		this.translation.IsVisible = this.Services.Pose.Gizmo == TransformHandleTypes.Translation;
		this.translation.Transform = transformSelection.WorldTransform;
		this.translation.Sensitivity = transformSelection.GizmoSensitivity;

		this.rotation.IsVisible = this.Services.Pose.Gizmo == TransformHandleTypes.Rotation;
		this.rotation.Transform = transformSelection.WorldTransform;
		this.rotation.Sensitivity = transformSelection.GizmoSensitivity;

		this.scale.IsVisible = this.Services.Pose.Gizmo == TransformHandleTypes.Scale;
		this.scale.Transform = transformSelection.WorldTransform;
		this.scale.Sensitivity = transformSelection.GizmoSensitivity;
	}

	private void OnTransformChanged(Transform newTransform)
	{
		if (this.selection is not TransformSelectionBase transformSelection)
			return;

		this.translation.Transform = transformSelection.WorldTransform;
		transformSelection.WorldTransform = newTransform;
	}
}

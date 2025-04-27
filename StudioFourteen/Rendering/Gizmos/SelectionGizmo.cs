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

namespace StudioFourteen.Rendering.Gizmos;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Selection;
using StudioFourteen.Services;

public class SelectionGizmo : GizmoBase
{
	private readonly DrawObject circle = new(Material.Line, Geometry.WireCircle);
	////private readonly DrawObject line = new(Material.Line, Geometry.WireCircle);

	public SelectionGizmo()
	{
		this.Add(this.circle);
	}

	public unsafe override void Draw(Transform transform, DrawState drawState)
	{
		SelectionBase? currentHover = this.Services.Selection.Hover;
		if (currentHover == null)
		{
			return;
		}

		if (currentHover is ObjectTableSelection objectSelection)
		{
			GameObject* gameObject = this.Services.GameObjects.Get(objectSelection.ObjectTableId);
			if (gameObject == null || gameObject->DrawObject == null)
				return;

			this.Transform = Transform.FromScale(gameObject->HitboxRadius / 2, 1, gameObject->HitboxRadius / 2);
			this.Transform *= Transform.FromTRS(gameObject->DrawObject->Position, gameObject->DrawObject->Rotation, gameObject->DrawObject->Scale);
		}
		else if (currentHover is BoneSelection boneSelection)
		{
			this.Transform = Transform.FromScale(0.15f, 1, 0.15f);
			this.Transform *= boneSelection.WorldTransform;
		}
		else if (currentHover is TransformSelectionBase transformSelection)
		{
			this.Transform = transformSelection.WorldTransform;
		}

		base.Draw(transform, drawState);
	}
}
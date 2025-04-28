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

namespace StudioFourteen.Rendering.Gizmos.Handles;

using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Selection;

public class HandleBase : GizmoBase
{
	private readonly DrawObject circle = new(Material.Line, Geometry.WireCircle);

	public HandleBase()
	{
		this.Add(this.circle);
	}

	// TODO: a handle service for doing the hit test dispatch, instead of cramming it
	// into the render pass?

	/*
	// Test
		Vector2? mouse = this.Services.Input.Mouse?.GetPosition();
		if (mouse != null)
		{
			HitTestResult result = new();
			this.Geometry.HitTest(mouse.Value, ref result);

			this.Log.Information($">> {result.DrawObject} {result.Distance}");
		}
		*/

	public unsafe override void Draw(Transform transform, DrawState drawState)
	{
		SelectionBase? currentSelection = this.Services.Selection.Current;
		if (currentSelection == null)
			return;

		if (currentSelection is ObjectTableSelection objectSelection)
		{
			GameObject* gameObject = this.Services.GameObjects.Get(objectSelection.ObjectTableId);
			if (gameObject == null || gameObject->DrawObject == null)
				return;

			this.Transform = Transform.FromScale(gameObject->HitboxRadius / 2, 1, gameObject->HitboxRadius / 2);
			this.Transform *= Transform.FromTRS(gameObject->DrawObject->Position, gameObject->DrawObject->Rotation, gameObject->DrawObject->Scale);
		}

		base.Draw(transform, drawState);
	}
}
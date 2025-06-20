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

namespace StudioFourteen.Scene.GameObjects.Characters.Skeletons;

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using StudioFourteen.Rendering.Draw.Gizmos;
using StudioFourteen.Scene.GameObjects;
using StudioFourteen.Scene.GameObjects.Characters;
using StudioFourteen.Services;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class SkeletonGizmo : SceneObjectGizmoBase
{
	private bool isInitialized = false;

	public override string Name => "Skeleton";
	public override object? Icon => Resources.Find("ICON_Gizmo_CharacterSkeleton");
	public override bool KeepScreenSize => false;

	public int ObjectTableIndex
	{
		get
		{
			if (this.sceneObject is GameObject obj)
				return obj.ObjectIndex;

			return 0;
		}
	}

	public unsafe void Initialize()
	{
		if (this.sceneObject is Skeleton skeleton)
		{
			foreach (SkeletonBone bone in skeleton.Bones)
			{
				SkeletonBoneGizmo gizmo = new(bone);
				this.Add(gizmo);
			}
		}

		this.isInitialized = this.Children.Count > 0;
	}

	protected unsafe override void OnDraw()
	{
		if (!this.IsVisible)
			return;

		if (!this.isInitialized)
			this.Initialize();

		if (this.sceneObject is Skeleton skeleton)
		{
			XivCharacter* pCharacter = (XivCharacter*)skeleton.GetXivGameObject();
			if (pCharacter == null || pCharacter->DrawObject == null)
				return;

			float scale = pCharacter->GetCharacterScale();
			Transform modelTransform = StudioFourteen.Transform.FromTRS(
				pCharacter->DrawObject->Position,
				pCharacter->DrawObject->Rotation,
				pCharacter->DrawObject->Scale * scale);

			this.Transform = modelTransform;
		}

		base.OnDraw();
	}
}

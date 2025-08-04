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

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;

public class SkeletonGizmo : SceneObjectGizmoBase<Skeleton>
{
	private readonly Dictionary<SkeletonBone, SkeletonBoneGizmo> boneGizmos = new();

	public SkeletonGizmo(GameObject gameObject)
	{
		this.Enable(gameObject);
	}

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

	protected unsafe override void OnDraw()
	{
		if (this.SceneObject == null || !this.IsVisible)
			return;

		if (this.boneGizmos.Count != this.SceneObject.Bones.Count)
		{
			HashSet<SkeletonBone> toRemove = new(this.boneGizmos.Keys);
			foreach (SkeletonBone bone in this.SceneObject.Bones)
			{
				if (this.boneGizmos.ContainsKey(bone))
				{
					toRemove.Remove(bone);
				}
				else
				{
					SkeletonBoneGizmo gizmo = new(bone);
					this.boneGizmos.Add(bone, gizmo);
					this.Add(gizmo);
				}
			}

			foreach (SkeletonBone boneId in toRemove)
			{
				SkeletonBoneGizmo gizmo = this.boneGizmos[boneId];
				this.Remove(gizmo);
				this.boneGizmos.Remove(boneId);
			}
		}

		XivCharacter* pCharacter = (XivCharacter*)this.SceneObject.GetXivGameObject();
		if (pCharacter == null || pCharacter->DrawObject == null)
			return;

		float scale = pCharacter->GetCharacterScale();
		Transform modelTransform = StudioFourteen.Transform.FromTRS(
			pCharacter->DrawObject->Position,
			pCharacter->DrawObject->Rotation,
			pCharacter->DrawObject->Scale * scale);

		this.Transform = modelTransform;

		base.OnDraw();
	}
}

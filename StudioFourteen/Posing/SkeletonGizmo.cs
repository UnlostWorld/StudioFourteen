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

using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Selection;
using StudioFourteen.Services;

public class SkeletonGizmo : DrawGroup
{
	public readonly int ObjectTableIndex;

	private bool isInitialized = false;

	public SkeletonGizmo(int objectTableIndex)
	{
		this.ObjectTableIndex = objectTableIndex;
	}

	public unsafe void Initialize()
	{
		HashSet<string> boneNames = this.Services.Pose.GetAllBoneNames(this.ObjectTableIndex);
		foreach (string boneName in boneNames)
		{
			if (this.Services.Settings.Current.HideGenitals
				&& this.Services.Content.GenitalBones?.Contains(boneName) == true)
				continue;

			BoneSceneObject? selection = this.Services.Pose.FindBone(this.ObjectTableIndex, boneName);

			if (selection == null)
				continue;

			BoneGizmo gizmo = new(selection);
			this.Add(gizmo);
		}

		this.isInitialized = this.Children.Count > 0;
	}

	protected unsafe override void OnDraw()
	{
		this.IsVisible = false;

		if (ServiceManager.Instance.GroupPose.IsGroupPosing
			&& (this.ObjectTableIndex < GroupPoseService.GPoseFirstCharacter
			|| this.ObjectTableIndex > GroupPoseService.GPoseFirstCharacter + GroupPoseService.GPoseCharacterCount))
		{
			return;
		}

		// Add this as a gizmo option.
		if (this.Services.Target.TargetObjectIndex != this.ObjectTableIndex)
			return;

		Character* pCharacter = ServiceManager.Instance.GameObjects.Get<Character>(this.ObjectTableIndex);
		if (pCharacter == null || pCharacter->DrawObject == null)
			return;

		if (!pCharacter->CanDraw())
			return;

		if (pCharacter->ObjectKind != ObjectKind.Pc
			&& pCharacter->ObjectKind != ObjectKind.BattleNpc
			&& pCharacter->ObjectKind != ObjectKind.EventNpc)
			return;

		if (!this.isInitialized)
			this.Initialize();

		float scale = pCharacter->GetCharacterScale();
		Transform modelTransform = StudioFourteen.Transform.FromTRS(
			pCharacter->DrawObject->Position,
			pCharacter->DrawObject->Rotation,
			pCharacter->DrawObject->Scale * scale);

		this.Transform = modelTransform;

		this.IsVisible = true;
		base.OnDraw();
	}
}

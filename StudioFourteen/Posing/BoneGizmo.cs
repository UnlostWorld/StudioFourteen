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
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using Lumina.Excel.Sheets;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Materials;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Selection;

public class BoneGizmo : SelectionHandle
{
	private readonly BoneSceneObject boneSelection;
	private readonly BoneId boneId;
	private readonly BoneId? parentBoneId;
	private readonly MeshRenderer<BoneCapMaterial> capRenderer;
	private readonly LineRenderer<BoneMaterial>? connectionRenderer;

	public BoneGizmo(BoneSceneObject selection)
		: base(selection)
	{
		this.boneSelection = selection;
		this.capRenderer = new(MeshContent.Bone);
		this.Add(this.capRenderer);

		foreach ((BoneId boneId, List<BoneId> path) in selection.BonePaths)
		{
			this.boneId = boneId;

			if (path.Count > 0)
			{
				if (path[0].BoneIndex == 0)
					continue;

				this.connectionRenderer = new();
				this.connectionRenderer.IsHitTestVisible = false;
				this.Add(this.connectionRenderer);
				this.parentBoneId = path[0];
			}

			break;
		}
	}

	protected unsafe override void OnDraw()
	{
		base.OnDraw();

		if (!this.boneId.Resolve(
			out Character* pCharacter,
			out Skeleton* pSkeleton,
			out PartialSkeleton* pPartialSkeleton,
			out hkaPose* pPose))
			return;

		if (!pCharacter->CanDraw())
			return;

		if (this.boneId.BoneIndex >= pPose->Skeleton->Bones.Length)
			return;

		Transform boneTransform = *pPose->AccessBoneModelSpace(this.boneId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
		this.capRenderer.Transform = boneTransform;

		bool isHoveredOrSelected = this.IsHovered | this.IsSelected;

		this.capRenderer.Material.Size = isHoveredOrSelected ? 2.0f : 1.0f;
		this.capRenderer.Material.DepthOffset = isHoveredOrSelected ? 0.001f : 0f;
		////this.capRenderer.Material.Color = this.IsSelected ? Color.White : new(1, 0, 0, 1);

		Vector3 bonePos = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());

		if (this.connectionRenderer != null && this.parentBoneId != null)
		{
			if (this.parentBoneId.Value.BoneIndex >= pPose->Skeleton->Bones.Length)
				return;

			Transform childModelSpaceTransform = *pPose->AccessBoneModelSpace(this.parentBoneId.Value.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			Vector3 childPos = Vector3.Transform(Vector3.Zero, childModelSpaceTransform.ToMatrix());
			this.connectionRenderer.To = bonePos;
			this.connectionRenderer.From = childPos;
		}
	}
}
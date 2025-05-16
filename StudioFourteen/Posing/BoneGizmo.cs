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
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Scene;
using StudioFourteen.Rendering.Scene.Handles;
using StudioFourteen.Selection;

using Material = StudioFourteen.Rendering.Material;

public class BoneGizmo : Handle
{
	private readonly BoneId boneId;
	private readonly BoneId? parentBoneId;
	private readonly MeshRenderer capRenderer;
	private readonly LineRenderer? connectionRenderer;
	private readonly BoneSelection selection;

	public BoneGizmo(BoneSelection selection)
	{
		this.selection = selection;

		this.capRenderer = new(Meshes.Bone, Material.BoneCap);
		this.Add(this.capRenderer);

		foreach ((BoneId boneId, List<BoneId> path) in this.selection.BonePaths)
		{
			this.boneId = boneId;

			if (path.Count > 0)
			{
				this.connectionRenderer = new(Material.Bone);
				this.connectionRenderer.IsHitTestVisible = false;
				this.Add(this.connectionRenderer);
				this.parentBoneId = path[0];
			}

			break;
		}
	}

	public override void OnHover(bool hover)
	{
		base.OnHover(hover);
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

		Material.BoneCap.GetInstanceData(this.capRenderer).Size = this.IsHovered ? 2.0f : 1.0f;
		Material.BoneCap.GetInstanceData(this.capRenderer).DepthOffset = this.IsHovered ? 0.001f : 0f;

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
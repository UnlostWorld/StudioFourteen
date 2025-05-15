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

using Material = StudioFourteen.Rendering.Material;

public class BoneGizmo : SceneGroup
{
	private readonly BoneId boneId;
	private readonly MeshRenderer capRenderer;
	private readonly Dictionary<BoneId, LineRenderer> connectionRenderers = new();

	public BoneGizmo(BoneId boneId)
	{
		this.boneId = boneId;
		this.capRenderer = new(Meshes.Bone, Material.BoneCap);
		this.Add(this.capRenderer);
	}

	protected bool IsObjectTargeted => ServiceManager.Instance.Target.TargetObjectIndex == this.boneId.ObjectTableIndex;
	protected bool IsMouseOver { get; private set; }

	public void AddChild(BoneId childId)
	{
		LineRenderer renderer = new(Material.Bone);
		renderer.IsHitTestVisible = false;
		this.Add(renderer);
		this.connectionRenderers.Add(childId, renderer);
	}

	public override void OnHit(HitTestResult result)
	{
		result.Handled = true;
		this.IsMouseOver = true;
		base.OnHit(result);
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

		this.capRenderer.Color = this.IsMouseOver ? Color.Black : Color.White;

		Vector3 bonePos = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());

		foreach ((BoneId childId, LineRenderer renderer) in this.connectionRenderers)
		{
			if (childId.BoneIndex >= pPose->Skeleton->Bones.Length)
				continue;

			Transform childModelSpaceTransform = *pPose->AccessBoneModelSpace(childId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

			Vector3 childPos = Vector3.Transform(Vector3.Zero, childModelSpaceTransform.ToMatrix());

			renderer.From = bonePos;
			renderer.To = childPos;
		}
	}
}
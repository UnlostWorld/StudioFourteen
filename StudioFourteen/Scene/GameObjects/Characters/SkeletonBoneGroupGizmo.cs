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

using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Rendering.Materials;
using System.Collections.Generic;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using XivSkeleton = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

public class SkeletonBoneGroupGizmo : SelectionHandle
{
	private readonly MeshRenderer<BoneCapMaterial> capRenderer;
	private readonly SkeletonBoneGroup boneGroup;
	private readonly List<SkeletonBoneGizmo> boneGizmos = new();

	private float transitionProgress = 0;

	public SkeletonBoneGroupGizmo(SkeletonBoneGroup selection)
		: base(selection)
	{
		this.boneGroup = selection;
		this.capRenderer = new(MeshContent.Bone);
		this.Add(this.capRenderer);
	}

	public override bool CanDrag => false;

	public void AddBone(SkeletonBoneGizmo bone)
	{
		this.boneGizmos.Add(bone);
	}

	protected unsafe override void OnDraw()
	{
		bool isAnyBoneHovered = this.IsHovered;
		if (!isAnyBoneHovered)
		{
			foreach (SkeletonBoneGizmo bone in this.boneGizmos)
			{
				if (bone.IsHovered)
				{
					isAnyBoneHovered = true;
					break;
				}
			}
		}

		// I'm so proud of her.
		this.transitionProgress = float.Lerp(this.transitionProgress, isAnyBoneHovered ? 1.0f : 0.0f, 0.25f);

		this.capRenderer.Material.Size = float.Lerp(1.25f, 10, this.transitionProgress);
		this.capRenderer.Material.Color = new Color(1.0f, 0.75f, 0.75f, 1.0f - this.transitionProgress);
		this.capRenderer.Material.DepthOffset = 0.001f * this.transitionProgress;

		if (this.boneGroup.Bones == null)
			return;

		// TODO: spread!
		foreach (SkeletonBoneGizmo bone in this.boneGizmos)
		{
			bone.GroupAlphaMultiplier = this.transitionProgress;
			bone.GroupDepthOffset = 0.002f * this.transitionProgress;
		}

		Vector3? pos = null;
		int count = 0;

		foreach (SkeletonBone bone in this.boneGroup.Bones)
		{
			if (!bone.PrimaryBoneId.Resolve(
				out XivCharacter* pCharacter,
				out XivSkeleton* pSkeleton,
				out PartialSkeleton* pPartialSkeleton,
				out hkaPose* pPose))
				continue;

			if (!pCharacter->CanDraw())
				continue;

			if (bone.PrimaryBoneId.BoneIndex >= pPose->Skeleton->Bones.Length)
				continue;

			if (bone.BoneName == "n_root" || bone.BoneName == "n_throw")
				continue;

			// TODO: average
			Transform boneTransform = *pPose->AccessBoneModelSpace(bone.PrimaryBoneId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);

			if (Matrix4x4.Decompose(
				boneTransform.ToMatrix(),
				out Vector3 scale,
				out Quaternion rotation,
				out Vector3 translation))
			{
				count++;
				if (pos == null)
				{
					pos = translation;
				}
				else
				{
					pos += translation;
				}
			}
		}

		if (pos != null && count > 0)
		{
			pos /= count;
			this.capRenderer.Transform = StudioFourteen.Transform.FromTranslation((Vector3)pos);
		}

		base.OnDraw();
	}
}

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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using StudioFourteen.Rendering;
using StudioFourteen.Rendering.Draw;
using StudioFourteen.Rendering.Draw.Handles;
using StudioFourteen.Rendering.Materials;

using XivCharacter = FFXIVClientStructs.FFXIV.Client.Game.Character.Character;
using XivSkeleton = FFXIVClientStructs.FFXIV.Client.Graphics.Render.Skeleton;

public class SkeletonBoneGizmo : SelectionHandle
{
	private readonly SkeletonBone skeletonBone;
	private readonly BoneId boneId;
	private readonly MeshRenderer<BoneCapMaterial> capRenderer;
	private readonly LineRenderer<BoneMaterial> connectionRenderer;
	private float alpha = 0;

	public SkeletonBoneGizmo(SkeletonBone selection)
		: base(selection)
	{
		this.boneId = selection.PrimaryBoneId;
		this.skeletonBone = selection;
		this.capRenderer = new(MeshContent.Bone);
		this.Add(this.capRenderer);

		this.connectionRenderer = new();
		this.connectionRenderer.IsHitTestVisible = false;
		this.Add(this.connectionRenderer);
	}

	protected unsafe override void OnDraw()
	{
		base.OnDraw();

		if (!this.boneId.Resolve(
			out XivCharacter* pCharacter,
			out XivSkeleton* pSkeleton,
			out PartialSkeleton* pPartialSkeleton,
			out hkaPose* pPose))
			return;

		if (!pCharacter->CanDraw())
			return;

		if (this.boneId.BoneIndex >= pPose->Skeleton->Bones.Length)
			return;

		if (this.skeletonBone.BoneName == "n_root" || this.skeletonBone.BoneName == "n_throw")
			this.IsVisible = false;

		// If this is genital and hide genitals!
		if (this.Services.Settings.Current.HideGenitals && this.Services.Content.GenitalBones?.Contains(this.skeletonBone.BoneName) == true)
			this.IsVisible = false;

		Transform boneTransform = *pPose->AccessBoneModelSpace(this.boneId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
		this.capRenderer.Transform = boneTransform;

		bool isAnyParentSelected = false;
		bool isAnyParentHovered = false;
		SkeletonBone? parent = this.skeletonBone.Parent;
		while (parent != null)
		{
			isAnyParentSelected |= parent.IsSelected;
			isAnyParentHovered |= parent.IsHovered;
			parent = parent.Parent;
		}

		bool isAnyBoneSelected = this.Services.Selection.Current is SkeletonBone;
		bool isAnyBoneHovered = this.Services.Selection.Hover is SkeletonBone;
		bool isHoveredOrSelected = this.IsHovered | this.IsSelected;

		this.capRenderer.Material.Size = isHoveredOrSelected ? 2.0f : 1.0f;
		this.capRenderer.Material.DepthOffset = isHoveredOrSelected ? 0.001f : 0f;

		float desiredAlpha = 0.1f;
		if (this.IsSelected || isAnyParentSelected)
		{
			desiredAlpha = 1.0f;
			this.capRenderer.Material.Color = new(1.0f, 0.08f, 0.58f, 1.0f);
		}
		else if (this.IsHovered || isAnyParentHovered)
		{
			desiredAlpha = 1.0f;
			this.capRenderer.Material.Color = Color.White;
		}
		else if (isAnyBoneSelected || isAnyBoneHovered)
		{
			this.capRenderer.Material.Color = new(1, 1, 1, 0.1f);
		}
		else
		{
			desiredAlpha = 1.0f;
			this.capRenderer.Material.Color = Color.White;
		}

		this.alpha = float.Lerp(this.alpha, desiredAlpha, 0.25f);
		this.capRenderer.Material.Color.A = this.alpha;

		Vector3 bonePos = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());

		if (this.skeletonBone.Parent != null)
		{
			BoneId boneId = this.skeletonBone.Parent.PrimaryBoneId;
			if (boneId.BoneIndex >= pPose->Skeleton->Bones.Length)
				return;

			if (this.skeletonBone.Parent.BoneName == "n_root" || this.skeletonBone.Parent.BoneName == "n_throw")
				return;

			// If this is genital and hide genitals!
			if (this.Services.Settings.Current.HideGenitals && this.Services.Content.GenitalBones?.Contains(this.skeletonBone.Parent.BoneName) == true)
				return;

			Transform childModelSpaceTransform = *pPose->AccessBoneModelSpace(boneId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			Vector3 childPos = Vector3.Transform(Vector3.Zero, childModelSpaceTransform.ToMatrix());
			this.connectionRenderer.To = bonePos;
			this.connectionRenderer.From = childPos;

			if (isAnyParentSelected)
			{
				this.connectionRenderer.Material.Color = new(1.0f, 0.08f, 0.58f, 1.0f);
				this.connectionRenderer.Material.Thickness = 1.25f;
			}
			else if (isAnyParentHovered)
			{
				this.connectionRenderer.Material.Color = Color.White;
				this.connectionRenderer.Material.Thickness = 1.25f;
			}
			else if (isAnyBoneSelected)
			{
				this.connectionRenderer.Material.Color = new(0.5f, 0.5f, 0.5f, 0.1f);
			}
			else
			{
				this.connectionRenderer.Material.Color = new(0.5f, 0.5f, 0.5f, 1.0f);
			}

			this.connectionRenderer.Material.Color.A = this.alpha;
		}
	}
}
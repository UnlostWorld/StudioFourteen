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
	public float GroupAlpha = 1;
	public float GroupDepthOffset = 0;
	public Vector3? GroupOverridePosition = null;

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

	public override bool CanDrag => false;
	public string BoneName => this.skeletonBone.BoneName;

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
		////if (SettingsService.Current.HideGenitals && ContentService.GenitalBones?.Contains(this.skeletonBone.BoneName) == true)
		////	this.IsVisible = false;

		Vector3 bonePos = Vector3.Zero;
		if (this.GroupOverridePosition == null)
		{
			Transform boneTransform = *pPose->AccessBoneModelSpace(this.boneId.BoneIndex, hkaPose.PropagateOrNot.DontPropagate);
			this.capRenderer.Transform = boneTransform;
			bonePos = Vector3.Transform(Vector3.Zero, boneTransform.ToMatrix());
		}
		else
		{
			bonePos = (Vector3)this.GroupOverridePosition;
			this.capRenderer.Transform = Transform.FromTranslation(bonePos);
		}

		bool isAnyParentSelected = false;
		bool isAnyParentHovered = false;
		SkeletonBone? parent = this.skeletonBone.Parent;
		while (parent != null)
		{
			isAnyParentSelected |= parent.IsSelected;
			isAnyParentHovered |= parent.IsHovered;
			parent = parent.Parent;
		}

		bool isAnyBoneSelected = SelectionService.Current is SkeletonBone || SelectionService.Current is SkeletonBoneGroup;
		bool isAnyBoneHovered = SelectionService.Hover is SkeletonBone || SelectionService.Hover is SkeletonBoneGroup;
		bool isHoveredOrSelected = this.IsHovered | this.IsSelected;
		bool isAnyHandleDragging = this.CurrentRenderer?.Input.CurrentPress?.IsDragging == true;

		bool isGroupHovered = false;
		if (SelectionService.Hover is SkeletonBoneGroup hoverGroup)
		{
			isGroupHovered = hoverGroup.Contains(this.Selection);
		}

		bool isGroupSelected = false;
		if (SelectionService.Current is SkeletonBoneGroup currentGroup)
		{
			isGroupSelected = currentGroup.Contains(this.Selection);
		}

		this.capRenderer.Material.Size = isHoveredOrSelected ? 2.0f : 1.0f;
		this.capRenderer.Material.DepthOffset = (isHoveredOrSelected ? 0.001f : 0f) + this.GroupDepthOffset;

		float desiredAlpha = 0.1f;
		if (isAnyHandleDragging)
		{
			desiredAlpha = 0.0f;
		}
		else if (this.IsSelected || isAnyParentSelected || isGroupSelected)
		{
			desiredAlpha = 1.0f;
			this.capRenderer.Material.Color = new(1.0f, 0.08f, 0.58f, 1.0f);
		}
		else if (this.IsHovered || isAnyParentHovered || isGroupHovered)
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

		if (this.GroupOverridePosition != null)
		{
			this.capRenderer.Material.Color.A = this.GroupAlpha;
			this.IsVisible = this.GroupAlpha > 0.01f;
		}

		if (this.skeletonBone.Parent != null)
		{
			BoneId boneId = this.skeletonBone.Parent.PrimaryBoneId;
			if (boneId.BoneIndex >= pPose->Skeleton->Bones.Length)
				return;

			if (this.skeletonBone.Parent.BoneName == "n_root" || this.skeletonBone.Parent.BoneName == "n_throw")
				return;

			// If this is genital and hide genitals!
			////if (SettingsService.Current.HideGenitals && ContentService.GenitalBones?.Contains(this.skeletonBone.Parent.BoneName) == true)
			////	return;

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

			this.connectionRenderer.Material.Color.A = this.alpha * this.GroupAlpha;

			if (this.GroupOverridePosition != null)
			{
				this.connectionRenderer.Material.Color.A = 0;
			}
		}
	}
}